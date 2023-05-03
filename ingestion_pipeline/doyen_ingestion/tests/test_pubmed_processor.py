from io import BytesIO

import pytest
from click.testing import CliRunner
from elasticsearch import Elasticsearch
from pytest_mock import MockFixture

from doyen_ingestion.pubmed_processor import CONFIG, doyen_ingest_cli


@pytest.fixture
def es_mock(mocker: MockFixture):
    CONFIG.set("elasticsearch", "host", "FAKE_HOST")
    CONFIG.set("elasticsearch", "ca_certs", "NOPE")
    CONFIG.set("elasticsearch", "password", "NIL")

    from elasticsearch._sync.client import IndicesClient

    es = mocker.MagicMock(spec=Elasticsearch)
    es.indices = mocker.MagicMock(spec=IndicesClient)
    es.indices.create.side_effect = lambda *args, **kwargs: None
    es.indices.delete.side_effect = lambda *args, **kwargs: None
    es.bulk.side_effect = lambda *args, **kwargs: None

    mocker.patch("elasticsearch.Elasticsearch", return_value=es)
    return es


@pytest.fixture
def ftp_mock(mocker: MockFixture):
    # Read in the contents of the local gzipped XML file
    with open("pubmed_sample.xml.gz", "rb") as f:
        xml_data = f.read()

    # Create a BytesIO object from the XML data
    file_obj = BytesIO(xml_data)

    # Create a MagicMock for the FTP library's FTP object
    ftp_mock = mocker.MagicMock()

    # Mock the FTP object's login() method to always return True
    ftp_mock.login.return_value = True

    # Mock the FTP object's retrbinary() method to return the contents of the local XML file
    def mock_retrbinary(cmd, callback=mocker.MagicMock(), **kwargs):
        return callback(xml_data)

    ftp_mock.retrbinary.side_effect = mock_retrbinary

    # Mock the FTP object's list() method to return a list of files
    ftp_mock.nlst.return_value = ["pubmed_sample.xml.gz"]
    ftp_mock.mlsd.return_value = [("pubmed_sample.xml.gz", {"modify": 123459879})]

    mocker.patch("ftplib.FTP", return_value=ftp_mock)

    # Return the FTP mock
    return ftp_mock


@pytest.fixture
def runner():
    return CliRunner()


def test_upload(es_mock, ftp_mock, runner):
    result = runner.invoke(doyen_ingest_cli)
    assert result.exit_code == 0, result.return_value
    assert ftp_mock.mlsd.called
    assert ftp_mock.retrbinary.called
    assert es_mock.indices.exists.called
    assert es_mock.indices.create.called
    assert es_mock.options.called
    assert len(es_mock.options.mock_calls) > 10
