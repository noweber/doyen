# Doyen Analysis Tools

This module contains tools for analyzing the results of queries to ElasticSearch. The goal is to use these tools
to develop better sorting and ranking algorithms for the production application. The main tool provided here
is the `network_analysis.py` file, with the `Search` class.

You will first want to install the `doyen_ingestion` package if you haven't already, which is also in this repo:
```bash
cd ../ingestion_pipeline
pip install -e .
cd -
```

You will also need to follow the [instructions in the ingestion pipeline](../ingestion_pipeline) for setting 
up ElasticSearch and ingesting the PubMed Data (at least a sample of it).

These tools come with some extra requirements in addition, which you can install with:
```bash
pip install -r requirements.txt
```

The tool is designed for us in a pylab environment, which requires `matplotlib`:
```bash
ipython --pylab
```