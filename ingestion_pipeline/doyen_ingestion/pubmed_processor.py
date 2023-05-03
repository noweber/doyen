import configparser
import json
import logging
import shutil
from datetime import datetime
from pathlib import Path
from typing import List, Optional

import click
import elasticsearch
from elasticsearch import helpers
from indra.literature.pubmed_client import get_metadata_from_xml_tree

from doyen_ingestion.ftp_client import NihFtpClient

# Define file paths relative to this location.
HERE = Path(__file__).parent.absolute()
CONFIG_FILE = HERE / "resources" / "config.ini"
if not CONFIG_FILE.exists():
    # Allow users that install this through pip or similar
    # to locate the config file in their home directory.
    CONFIG_FILE = Path("~/.doyen/config.ini").expanduser()

CONFIG_TEMPLATE = HERE / "resources" / "config_template.ini"
ES_INDEX_CONFIG = HERE / "resources" / "elastic-search-config.json"

# Create a logger with nice time stamps.
logging.basicConfig(
    level=logging.INFO, format=f"[%(asctime)s] %(name)s %(levelname)s - %(message)s"
)
logger = logging.getLogger("doyen_pubmed_upload")


# Make sure the user fills out the config file.
if not CONFIG_FILE.exists():
    logger.warning(f"No config file found. It should be at {CONFIG_FILE}.")
    CONFIG_FILE.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy(CONFIG_TEMPLATE, CONFIG_FILE)
    logger.warning(
        "Created a new config file from a template. Please "
        "update the config with current values."
    )
    input("After you update the values, press any key to continue..")


# Load the config.
CONFIG = configparser.ConfigParser()
CONFIG.read(CONFIG_FILE)


def get_es_client():
    """Get an instance of the elasticsearch client."""
    verify_certs_str = CONFIG.get("elasticsearch", "verify_certs")
    if verify_certs_str.lower() not in ("true", "false"):
        raise ValueError(
            f"Invalid value for verify_certs: {verify_certs_str}. "
            f"Must be either True or False."
        )
    verify_certs = verify_certs_str.lower() == "true"
    return elasticsearch.Elasticsearch(
        CONFIG.get("elasticsearch", "host"),
        verify_certs=verify_certs,
        ca_certs=CONFIG.get("elasticsearch", "ca_certs"),
        basic_auth=(
            CONFIG.get("elasticsearch", "username"),
            CONFIG.get("elasticsearch", "password"),
        ),
    )


def create_pubmed_paper_index():
    """Create the pubmed paper index in elasticsearch."""
    es = get_es_client()

    with ES_INDEX_CONFIG.open() as file_handle:
        es_config = json.load(file_handle)

    # We need to delete the index first before building a new
    index_name = CONFIG.get("index", "name")
    if es.indices.exists(index=index_name):
        logger.info("Deleting the pre-existing index.")
        es.indices.delete(index=index_name)

    # Build the index.
    try:
        es.indices.create(index=index_name, body={"mappings": es_config["mappings"]})
    except elasticsearch.BadRequestError as err:
        logger.error("Failed to create the index.")
        logger.exception(err)
        return False

    return True


def index_pubmed_files(
    file_paths: List[str],
    min_year=None,
    refresh_index: bool = True,
):
    """Indexes all the gz files in the provided list_of_files parameter"""
    start = datetime.now()

    if refresh_index:
        logger.info("Rebuilding the index...")
        create_pubmed_paper_index()

    client = NihFtpClient("pubmed")
    es = get_es_client()

    # Loop over all files, extract the information and index in bulk
    files_indexed = 0
    articles_added = 0
    for i, file_path in enumerate(file_paths):
        logger.info(f"Processing file {i} of {len(file_paths)}: {file_path}")
        start_file = datetime.now()

        # Parse the XML
        xml_tree = client.get_xml_tree(file_path)
        articles_by_pmid = get_metadata_from_xml_tree(
            xml_tree,
            get_abstracts=True,
            mesh_annotations=True,
            prepend_title=True,
            detailed_authors=True,
            references_included="pmid",
        )

        # NOTE: we are skipping publications without a publication date. Based on
        # manual inspection, these appear to be pre-prints.
        recent_articles = [
            article
            for article in articles_by_pmid.values()
            if article["publication_date"]["year"]
            and article["publication_date"]["year"] > min_year
        ]
        xml_tree.clear()

        # Update the date format.
        for article in recent_articles:
            pub_date = article["publication_date"]
            article["publication_date"] = datetime(
                year=pub_date["year"], month=pub_date["month"], day=pub_date["day"]
            ).strftime("%Y-%m-%d")

        # Make sure we didn't filter out all the articles.
        if not recent_articles:
            logger.info(f"No recent articles found in {file_path}, continuing...")
            continue

        # Try to upload all these articles into ElasticSearch
        logger.info(f"Starting to index {len(recent_articles)} articles...")
        start_index = datetime.now()
        try:
            for is_ok, response in helpers.streaming_bulk(
                es,
                recent_articles,
                index=CONFIG.get("index", "name"),
                raise_on_error=True,
                request_timeout=int(CONFIG.get("elasticsearch", "timeout")),
            ):
                if not is_ok:
                    # If the indexing is not successful, log the error
                    identifier = response["create"]["_id"]
                    error = response["create"]["error"]
                    logger.error(f"Error processing document ID {identifier}")
                    logger.error(json.dumps(error, indent=4, ensure_ascii=False))
        except Exception as err:
            logger.error(f"Failed to add documents from {file_path}: {err}")
            logger.exception(err)
            continue

        total_time_taken_per_file = datetime.now() - start_file
        indexing_time_taken_per_file = datetime.now() - start_index
        logger.info(
            f"Completed file {i} of {len(file_paths)}, {file_path}, "
            f"in {total_time_taken_per_file} total, and {indexing_time_taken_per_file}"
        )

        files_indexed += 1
        articles_added += len(recent_articles)

    total_time_taken = datetime.now() - start
    logger.info(
        f"Successfully index {files_indexed} files, adding {articles_added} articles in {total_time_taken}."
    )


@click.command()
@click.option(
    "--start",
    "-s",
    default=None,
    help="The index in the list of baselines from which to start. The value "
    "can be either positive, or negative to measure from the end of the list.",
    type=int,
)
@click.option(
    "--end",
    "-e",
    default=None,
    help="The index in the list of baseline files at which to end. The value "
    "can be either positive, or negative to measure from teh end of the list.",
    type=int,
)
@click.option(
    "--quiet",
    "-q",
    is_flag=True,
    help="Optionally quiet the logs to only show warnings.",
)
@click.option(
    "--baseline-only",
    "-B",
    is_flag=True,
    help="Only index the baseline files. Note this is incompatible with the --updatefiles-only/-U flag.",
)
@click.option(
    "--updatefiles-only",
    "-U",
    is_flag=True,
    help="Only index the update files. Note this is incompatible with the --baseline-only/-B flag.",
)
@click.option(
    "--min-year",
    "-y",
    default=-5,
    help=(
        "The minimum year of articles to index. Defaults to 5 years ago. Negative values are measured "
        "back from the current year."
    ),
    type=int,
)
@click.option(
    "--no-refresh-index",
    is_flag=True,
    help="Optionally do not refresh the index before indexing the files.",
)
def doyen_ingest_cli(
    start: Optional[int],
    end: Optional[int],
    quiet: bool,
    baseline_only: bool,
    updatefiles_only: bool,
    min_year: int,
    no_refresh_index: bool,
):
    """This CLI helps manage building indexes of the PubMed baseline and update files into ElasticSearch.

    The CLI will manage the download and decompression of the files from the NIH FTP server, parse the XML,
    and index the articles into ElasticSearch.

    It is assumed you have a running instance of ElasticSearch, and the appropriate values have been entered
    into the config file.
    """
    # Set the log level.
    if quiet:
        logging.basicConfig(level=logging.WARNING)

    # Get the list of candidate files
    client = NihFtpClient("pubmed")
    candidate_files = []
    if not updatefiles_only:
        candidate_files += sorted(
            f"baseline/{fname}"
            for fname, _ in client.list("baseline")
            if fname.endswith(".gz")
        )
    if not baseline_only:
        candidate_files += sorted(
            f"updatefiles/{fname}"
            for fname, _ in client.list("updatefiles")
            if fname.endswith(".gz")
        )

    # If the year is negative, we need to get the year relative to the current year.
    if min_year < 0:
        min_year = datetime.now().year + min_year

    # Select the sub-list of files to index.
    files_to_index = candidate_files[start:end]
    num_baseline_files = len([f for f in files_to_index if f.startswith("baseline")])
    num_update_files = len([f for f in files_to_index if f.startswith("updatefiles")])
    click.echo(
        f"Indexing {len(files_to_index)} files ({num_baseline_files} baseline, {num_update_files} updatefiles),"
        f" going back as far as {min_year}."
    )
    click.echo(
        f"The index {'will *NOT*' if no_refresh_index else '*WILL*'} be refreshed before indexing the files."
    )

    # We are currently just playing with the 100 files at the end of the list.
    return index_pubmed_files(
        files_to_index,
        min_year=min_year,
        refresh_index=not no_refresh_index,
    )


if __name__ == "__main__":
    doyen_ingest_cli()
