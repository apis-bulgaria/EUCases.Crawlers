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
 * Description: Adds the binary content of a web page to its metadata so it may be accessed in an IndexWriter.
 */

package de.averbis.eucases.outlinkmeta.nutch.parse;

import org.apache.commons.codec.binary.Base64;
import org.apache.commons.lang.StringUtils;
import org.apache.hadoop.conf.Configuration;
import org.apache.nutch.parse.HTMLMetaTags;
import org.apache.nutch.parse.HtmlParseFilter;
import org.apache.nutch.parse.ParseResult;
import org.apache.nutch.protocol.Content;
import org.w3c.dom.DocumentFragment;

import de.averbis.eucases.outlinkmeta.nutch.common.AbstractOutlinkMeta;
import de.averbis.eucases.outlinkmeta.nutch.common.OutlinkMetaConfig;

public class OutlinkMetaBinaryParseFilter extends AbstractOutlinkMeta implements HtmlParseFilter {

	public OutlinkMetaBinaryParseFilter() {

		super();
	}


	@Override
	public void setConf(Configuration conf) {

		super.setConf(conf);
	}


	@Override
	public Configuration getConf() {

		return super.getConf();
	}


	@Override
	public ParseResult filter(Content content, ParseResult parseResult, HTMLMetaTags metaTags, DocumentFragment doc) {

		// If outlinkmeta is configured to provide the binary content for indexing and the url field is present
		// in the content metadata (see OutlinkMetaScoringFilter) then put the base64-encoded content to the parse metadata.
		if (this.getIndexBinary() && StringUtils.isNotEmpty(content.getMetadata().get(this.getUrlField()))) {
			parseResult.get(content.getUrl()).getData().getParseMeta().add(OutlinkMetaConfig.BINARY_CONTENT, Base64.encodeBase64String(content.getContent()));
		}

		return parseResult;
	}

}
