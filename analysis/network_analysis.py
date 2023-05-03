import logging
from datetime import datetime
from typing import Any, List

import gilda
import networkx as nx
from elasticsearch import logger as es_logger
from networkx.algorithms.components import connected_components
from pydantic import BaseModel

from doyen_ingestion.pubmed_processor import get_es_client

logging.basicConfig(
    level=logging.WARNING, format=f"[%(asctime)s] %(name)s %(levelname)s - %(message)s"
)

# Set the logging level of the elasticsearch module to WARNING
es_logger.setLevel(logging.WARNING)


class Affiliation(BaseModel):
    """An affiliation with a name and list of identifiers."""

    name: str
    identifiers: List[Any]


class Author(BaseModel):
    """An author with a first name, last name, and list of affiliations."""

    first_name: str
    last_name: str
    initials: str | None
    suffix: str | None
    affiliations: List[Affiliation]
    identifier: str | None

    papers: List["Paper"] = None

    def key(self):
        return f"{self.first_name} {self.last_name}"

    def score(self):
        """Return the score of this author."""
        return sum(p.score for p in self.papers)


class Paper(BaseModel):
    """A paper with a PMID, title, authors, and MeSH annotations."""

    pmid: str
    title: str
    authors: List[Author] = None
    mesh_annotations: List[dict]
    score: float

    def key(self):
        return f"{self.pmid} {self.title}"


class Search:
    """Search for papers and authors matching the given terms.

    For example, you could search for papers containing "COVID-19" and "breast cancer".
    This example assumes you are in an `ipython --pylab` session:

    >>> search = Search("COVID-19", "breast cancer")
    >>> fig, axes = subplots(3, 4)
    >>> for ax, graph in zip(axes.flatten(), search.iter_subgraphs()):
    >>>     search.plot_subgraph(graph, ax)

    Parameters
    ----------
    *search_terms : Tuple[str]
        Any number of search terms used to select relevant content. The terms will be
        converted to MeSH terms where possible, if `ground_terms` is True.
    max_papers : int
        The maximum number of papers to return. Defaults to 100.
    ground_terms : bool
        If True, convert the search terms to MeSH terms where possible. Otherwise, search
        for the terms directly. Defaults to True. NOTE: Mapping to mesh terms can be a
        slow process, so setting this to False can speed up the search process considerably.
    """

    def __init__(self, *search_terms: str, max_papers=100, ground_terms=True):
        self.search_terms = search_terms
        self.ground_terms = ground_terms
        self.authors = None
        self.papers = None
        self.graph = None

        self.__search(max_papers=max_papers)

    def __search(self, max_papers=100):
        """Search for papers and authors matching the given terms."""
        # Convert the terms we can to MeSH
        print("Grounding terms...", end="", flush=True)
        start_grounding = datetime.now()
        match_query_terms = []
        for term in self.search_terms:
            if self.ground_terms and (
                scored_matches := gilda.ground(term, namespaces=["MESH"])
            ):
                match_query_terms.append(
                    {"match": {"mesh_annotations.mesh": scored_matches[0].term.id}}
                )
            else:
                match_query_terms.append({"match": {"abstract": term}})
        print(f"done in {datetime.now() - start_grounding} seconds", flush=True)

        # Run the search against ElasticSearch
        print("Searching for papers...", end="", flush=True)
        start_search = datetime.now()
        es = get_es_client()
        resp = es.search(
            index="pubmed-paper-index",
            query={"bool": {"should": match_query_terms}},
            size=max_papers,
        )
        print(f"done in {datetime.now() - start_search} seconds", flush=True)

        # Convert the results into models for authors and papers
        print("Building models...", end="", flush=True)
        start_models = datetime.now()
        self.authors = {}
        self.papers = {}
        for hit in resp["hits"]["hits"]:
            paper = Paper(
                pmid=hit["_source"]["pmid"],
                title=hit["_source"]["title"],
                score=hit["_score"],
                mesh_annotations=hit["_source"]["mesh_annotations"],
                authors=[],
            )
            for author_info in hit["_source"]["authors"]:
                author = Author(**author_info)
                if author.key() not in self.authors:
                    author.papers = []
                    self.authors[author.key()] = author
                else:
                    author = self.authors[author.key()]
                author.papers.append(paper)
                paper.authors.append(author)

            self.papers[paper.key()] = paper
        print(f"done in {datetime.now() - start_models} seconds", flush=True)

        # Build the graph
        print("Building graph...", end="", flush=True)
        start_graph = datetime.now()
        self.graph = nx.Graph()
        for author in self.authors.values():
            for paper in author.papers:
                self.graph.add_edge(author.key(), paper.key())
        print(f"done in {datetime.now() - start_graph} seconds", flush=True)

        return

    def iter_subgraphs(self):
        """Iterate over the subgraphs of the graph."""
        for subgraph in connected_components(self.graph):
            yield self.graph.subgraph(subgraph)

    def plot_subgraph(self, graph, ax):
        """Plot the given subgraph."""

        pos = nx.spring_layout(graph, k=0.5)
        author_nodes = [a for a in graph.nodes if a in self.authors]
        nx.draw_networkx_nodes(
            graph,
            pos,
            nodelist=author_nodes,
            node_color="tab:red",
            node_size=[100 * len(self.authors[a].papers) for a in author_nodes],
            ax=ax,
        )
        paper_nodes = [p for p in graph.nodes if p in self.papers]
        nx.draw_networkx_nodes(
            graph,
            pos,
            nodelist=paper_nodes,
            node_color="tab:blue",
            node_size=[500 * self.papers[p] for p in paper_nodes],
            ax=ax,
        )
        nx.draw_networkx_edges(graph, pos, ax=ax)
