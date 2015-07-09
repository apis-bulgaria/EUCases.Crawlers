/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the EUPL V.1.1
 * https://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 *
 * @author Florian Schmedding
 *
 * Description: Indexer for EUCases. Sends the documents to a web service.
 */

package de.averbis.eucases.servicewriter.nutch.indexwriter;

import java.io.File;
import java.io.IOException;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.mapred.JobConf;
import org.apache.nutch.indexer.IndexWriter;
import org.apache.nutch.indexer.NutchDocument;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import de.averbis.eucases.servicewriter.nutch.common.EUCasesIndexWriterConfig;
import de.averbis.eucases.servicewriter.nutch.util.Article;

public class EUCasesIndexWriter implements IndexWriter {

	private final static Logger logger = LoggerFactory.getLogger(EUCasesIndexWriter.class);

	Configuration conf;

	private String serviceUrl;
	private String segmentsPath;
	private String outputPath;


	public EUCasesIndexWriter() {

		super();
	}


	@Override
	public void setConf(Configuration conf) {

		this.conf = conf;
		this.serviceUrl = this.conf.get(EUCasesIndexWriterConfig.SERVICE_URL);
		this.segmentsPath = this.conf.get(EUCasesIndexWriterConfig.SEGMENTS_PATH);
		this.outputPath = this.conf.get(EUCasesIndexWriterConfig.OUTPUT_PATH);
		if (this.serviceUrl == null) {
			String message = "Missing web service URL. Should be set in config or via -D "
					+ EUCasesIndexWriterConfig.SERVICE_URL;
			message += "\n" + this.describe();
			EUCasesIndexWriter.logger.error(message);
			throw new RuntimeException(message);
		}
	}


	@Override
	public Configuration getConf() {

		return this.conf;
	}


	@Override
	public void write(NutchDocument doc) throws IOException {

		Article article = new Article(doc);

		if (article.isUseful()) {
			System.out.println("Sending document " + article.getUrl());
			if (this.outputPath != null) {
				this.writeFiles(article);
			}
		}
	}


	private void writeFiles(Article article) {

		try {
			String journal = StringUtils.isNotEmpty(article.getJournal()) ? article.getJournal().replaceAll("\\s+", " ") : "undefined";
			article.writeXmlFile(new Path(this.outputPath, journal).toString(), article.getDigest() + ".meta.xml");

			FileUtils.writeByteArrayToFile(new File(new Path(this.outputPath, journal).toString(), article.getDigest() + "." + article.getFileExtension()), article.getContent());

			// ContentReader reader = new ContentReader(this.segmentsPath, this.conf);
			// reader.writeFile(article.getId(), article.getSegment(), new Path(this.outputPath, journal).toString(), article.getDigest() + ".pdf");
		} catch (IOException e) {
			EUCasesIndexWriter.logger.error("Error when writing files for article {}.", article.getId(), e);
		}
	}


	@Override
	public void delete(String key) throws IOException {

		System.out.println("Should delete document " + key);

	}


	@Override
	public void update(NutchDocument doc) throws IOException {

		this.write(doc);
	}


	@Override
	public void commit() throws IOException {

		// do nothing - there is no commit operation for the web service
	}


	@Override
	public void close() throws IOException {

		// do nothing - there are no pending documents
	}


	@Override
	public String describe() {

		StringBuffer sb = new StringBuffer("EUCasesIndexWriter\n");
		sb.append("\t").append(EUCasesIndexWriterConfig.SERVICE_URL).append(" : URL of the web service (mandatory)\n");
		sb.append("\t").append(EUCasesIndexWriterConfig.SEGMENTS_PATH).append(" : path to the segments\n");
		sb.append("\t").append(EUCasesIndexWriterConfig.OUTPUT_PATH).append(" : location for file output\n");
		return sb.toString();
	}


	@Override
	public void open(JobConf job, String name) throws IOException {

		// nothing to do
	}

}
