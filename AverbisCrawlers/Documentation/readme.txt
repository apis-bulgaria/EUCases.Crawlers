Introduction
============

The crawler from Averbis is based on Apache Nutch 1 and thus on Apache Hadoop. By using Hadoop 2.4.0 it is possible to run the crawler on both Linux and Windows platforms. 
The platform-dependent binaries necessary for Windows are included (only 64-bit version). The binary distribution contains all prerequisites to run the crawler.


Running the crawler
------------------

The crawler is executed as follows:
bin\crawl <seeds> <crawl path> <webservice url> <maximum number of iterations> <optional path to store the files>

Example:
bin\crawl seeds\eucases.txt crawl\eucases http://techno.eucases.eu/CrawlerService/ServiceEUCases.svc 50 download
