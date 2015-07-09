/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 *
 * @author Florian Schmedding
 *
 * Description: A nutch plugin that transfers metadata from the parsed document to an outlink. The metadata tags 
 * and the outlink can be configured. The outlink's metadata will be available in the processing of the linked document.
 */

package de.averbis.eucases.outlinkmeta.nutch.parse;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.Arrays;

import org.apache.commons.lang.StringUtils;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.io.MapWritable;
import org.apache.hadoop.io.Text;
import org.apache.nutch.indexer.NutchField;
import org.apache.nutch.metadata.Metadata;
import org.apache.nutch.parse.HTMLMetaTags;
import org.apache.nutch.parse.HtmlParseFilter;
import org.apache.nutch.parse.Outlink;
import org.apache.nutch.parse.Parse;
import org.apache.nutch.parse.ParseResult;
import org.apache.nutch.protocol.Content;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.DocumentFragment;

import de.averbis.eucases.outlinkmeta.nutch.common.AbstractOutlinkMeta;

public class OutlinkMetaParseFilter extends AbstractOutlinkMeta implements HtmlParseFilter {

	private final static Logger logger = LoggerFactory.getLogger(OutlinkMetaParseFilter.class);


	public OutlinkMetaParseFilter() {

		super();
	}


	@Override
	public ParseResult filter(Content content, ParseResult parseResult, HTMLMetaTags metaTags, DocumentFragment node) {

		if (!this.shouldProcess(content, parseResult)) {
			// OutlinkMetaParseFilter.logger.info("Not processing {}", content.getUrl());
			return parseResult;
		}

		OutlinkMetaParseFilter.logger.info("Processing {}", content.getUrl());

		Parse parse = parseResult.get(content.getUrl());
		Metadata metadata = parse.getData().getParseMeta();

		MapWritable annotations = this.createOutlinkAnnotations(metadata);

		String url = metadata.get(this.getUrlField());
		try {
			// make metadata url absolute
			URL baseUrl = new URL(content.getBaseUrl());
			url = (new URL(baseUrl, url)).toString();
			// update url in metadata (not really necessary because URL gets replaced in OutlinkMetaScroringFilter.initialScore())
			metadata.set(this.getUrlField(), url);
			Outlink annotatedOutlink = this.createAnnotatedOutlink(url, this.getUrlDescription(), annotations);
			Outlink[] outlinks = parse.getData().getOutlinks();
			parse.getData().setOutlinks(this.addOutlink(outlinks, annotatedOutlink));
		} catch (MalformedURLException e) {
			OutlinkMetaParseFilter.logger.warn("Malformed outlink url: {}", url);
		}

		return parseResult;
	}


	/**
	 * Creates the metadata for an outlink according to the configured fields. Empty fields are not added.
	 * 
	 * @param metadata
	 *            The metadata created by the previous parsers
	 * @return The metadata for the outlink
	 */
	private MapWritable createOutlinkAnnotations(Metadata metadata) {

		MapWritable md = new MapWritable();

		for (String field : this.getFields()) {
			NutchField nutchField = new NutchField();
			for (String value : metadata.getValues(field)) {
				nutchField.add(value);
			}
			if (nutchField.getValues().size() > 0) {
				md.put(new Text(field), nutchField);
			}
		}

		return md;
	}


	/**
	 * Creates a new outlink with metadata.
	 * 
	 * @param url
	 *            Url for the link
	 * @param description
	 *            Descrition (anchor) for the link
	 * @param annotations
	 *            Metadata for the link
	 * @return The new outlink
	 * @throws MalformedURLException
	 *             If the given url was bad
	 */
	private Outlink createAnnotatedOutlink(String url, String description, MapWritable annotations) throws MalformedURLException {

		Outlink annotatedOutlink = new Outlink(url, description);
		annotatedOutlink.setMetadata(annotations);
		return annotatedOutlink;
	}


	/**
	 * Adds one outlink to an outlink array. The given array will not be modified.
	 * 
	 * @param outlinks
	 *            An array with current outlinks
	 * @param outlink
	 *            The outlink to add
	 * @return A new array containing a copy of the given outlinks and the additional outlink on the last position
	 */
	private Outlink[] addOutlink(Outlink[] outlinks, Outlink outlink) {

		Outlink[] extendedOutlinks = Arrays.copyOf(outlinks, outlinks.length + 1);
		extendedOutlinks[extendedOutlinks.length - 1] = outlink;
		return extendedOutlinks;
	}


	@Override
	public void setConf(Configuration conf) {

		super.setConf(conf);

	}


	@Override
	public Configuration getConf() {

		return super.getConf();
	}


	/**
	 * Check whether the document should be processed. A document should be processed if the metadata contain a value for the key name in OutlinkMetaConfig.URL_FIELD.
	 * 
	 * @param content
	 * @param parseResult
	 * @return True if the document should be processed.
	 */
	private boolean shouldProcess(Content content, ParseResult parseResult) {

		Parse parse = parseResult.get(content.getUrl());
		Metadata metadata = parse.getData().getParseMeta();
		return StringUtils.isNotEmpty(metadata.get(this.getUrlField()));
	}

}
