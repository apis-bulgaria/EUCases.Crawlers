/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the EUPL V.1.1
 * https://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 *
 * @author Florian Schmedding
 *
 * Description: A reader for Nutch segments. It retrieves the content of fetched documents.
 */

package de.averbis.eucases.servicewriter.nutch.util;

import java.io.File;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.util.HashMap;
import java.util.List;

import org.apache.commons.io.FileUtils;
import org.apache.commons.io.IOUtils;
import org.apache.commons.io.output.NullOutputStream;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.Text;
import org.apache.hadoop.io.Writable;
import org.apache.nutch.protocol.Content;
import org.apache.nutch.segment.SegmentReader;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ContentReader {

	private final static Logger logger = LoggerFactory.getLogger(ContentReader.class);

	private final Path segments;

	private final Configuration conf;


	public ContentReader(String segmentsPath, Configuration conf) {

		this.segments = new Path(segmentsPath);
		this.conf = conf;
	}


	/**
	 * Get the binary content of a document
	 * 
	 * @param url
	 * @param segment
	 * @return
	 */
	public byte[] getContent(String url, String segment) {

		NullOutputStream nullStream = null;
		OutputStreamWriter nullWriter = null;
		try {
			SegmentReader reader = new SegmentReader(this.conf, true, false, false, false, false, false);
			Path segmentPath = new Path(this.segments, segment);

			nullStream = new NullOutputStream();
			nullWriter = new OutputStreamWriter(nullStream);
			HashMap<String, List<Writable>> map = new HashMap<String, List<Writable>>();
			reader.get(segmentPath, new Text(url), nullWriter, map);

			Content content = (Content) map.get("co").get(0);
			return content.getContent();
		} catch (Exception e) {
			ContentReader.logger.error("Content of {} could not be retrieved.", url, e);
			return new byte[0];
		} finally {
			IOUtils.closeQuietly(nullWriter);
			IOUtils.closeQuietly(nullStream);
		}
	}


	/**
	 * Utility method for writing the content to a file.
	 * 
	 * @param url
	 * @param segment
	 * @param filename
	 * @throws IOException
	 */
	public void writeFile(String url, String segment, String path, String filename) throws IOException {

		FileUtils.writeByteArrayToFile(new File(path, filename), this.getContent(url, segment));
	}
}
