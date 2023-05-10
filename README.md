# Doyen

A tool for finding experts using publicly available data sources such as pubmed. This
project uses ElasticSearch to store article data from [pubmed](https://pubmed.ncbi.nlm.nih.gov/), and
uses it to rapidly find and rank experts with specific expertise.

## ElasticSearch Setup Instructions

First, you will want to ingest the data from the [PubMed FTP server](https://ftp.ncbi.nlm.nih.gov/pubmed/baseline/).
This involves setting up an ElasticSearch instance, and then running the ingestion pipeline. You can find
the instructions for setting up ElasticSearch in the [ingestion pipeline sub-package](ingestion_pipeline).

## Next you will want to set up the REST API 

### URL & Swagger
https://doyen-api.azurewebsites.net/index.html

#### API Setup Instructions
- The API is developed using the .NET framework.
	- It's recommended to use Visual Studio or Visual Studio Code for development and/or running the app locally.
		- For Visual Studio, download the lastest VS 2022 version:
			- Windows: https://visualstudio.microsoft.com/vs/#download
			- Mac: https://visualstudio.microsoft.com/vs/mac/
		- For VS Code, download the latest version here: https://code.visualstudio.com/download
- Download and install .NET 7.0 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Download and install the lastest version of Docker Desktop to be used for local hosting: https://www.docker.com/products/docker-desktop/
	- Optionally you may download and install IIS Express for local hosting, as well: https://www.microsoft.com/en-us/download/details.aspx?id=48264
- After VS 2022 is installed:
	- Using Visual Studio Installer, install the following packages:
		- "ASP.NET and web development"
		- "Azure developement"
	- To run the API locally:
		- Using Visual Studio 2022, open the API solution file in Visual Studio 2022
			- Build / Run as Docker or IIS Express (if available to you).
		- Using Visual Studio Code, Ctrl + F5 with the project opened should run the app by default.
- To configure the app, these are the current parameters found within appsettings.json:
	- ApplicationInsights:
		- For logging, the app uses an Appication Insights resource, if available:
		- This resource could be setup here: https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview?tabs=net
		- "InstrumentationKey"
			- This is key from the App Insights resource which contains allows access for sending log data.
	- ElasticsearchSettings
		- "URL"
			- When connecting to Elasticsearch setup as describe above, this value should be a string including the protocol: "https://localhost:<<your port number>>/pubmed-paper-index/_search?typed_keys=true"
		- "Username"
			- "elastic"
		- "Password"
			- << password for Elasticsearch >>
		- "SearchRecordsLimit"
			- This is an integer value which limits the amount of top records queried at once.
			- The API will grab the top X records by relevancy when querying for experts depending on the value of this parameter.
		
## Front-End Setup Instructions

The front end client is implemented in another repository, which you can
find [here](https://github.com/DoyenTeam/doyenclient).


## Analysis and Experimentation

This repo also includes the beginnings of a [toolkit for analyzing the results you can get
from ElasticSearch](analysis). The idea is to be able to test queries in bulk, gather statistics, examine
graphs, and so on. Using these tools, you can develop better sorting and ranking algorithms for the
REST API.
