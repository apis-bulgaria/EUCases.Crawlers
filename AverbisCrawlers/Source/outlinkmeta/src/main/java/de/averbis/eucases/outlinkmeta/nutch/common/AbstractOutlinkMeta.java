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
 * Description: Abstract class containing the common configuration boiler plate for the outlinkmeta plugin classes.
 */

package de.averbis.eucases.outlinkmeta.nutch.common;

import org.apache.hadoop.conf.Configuration;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public abstract class AbstractOutlinkMeta {

	private final static Logger logger = LoggerFactory.getLogger(AbstractOutlinkMeta.class);

	private Configuration conf;

	/**
	 * The metadata key of the field containing the outlink that should be annotated.
	 */
	private String urlField;

	/**
	 * The anchor description for the outlink.
	 */
	private String urlDescription;

	/**
	 * The metadata keys of the fields that contain the annotations for the outlink.
	 */
	private String[] fields;

	/**
	 * Indicates if the binary content should be available for indexing.
	 */
	private boolean indexBinary;


	public String getUrlField() {

		return this.urlField;
	}


	public String getUrlDescription() {

		return this.urlDescription;
	}


	public String[] getFields() {

		return this.fields;
	}


	public boolean getIndexBinary() {

		return this.indexBinary;
	}


	public AbstractOutlinkMeta() {

		super();
	}


	public Configuration getConf() {

		return this.conf;
	}


	public void setConf(Configuration conf) {

		this.conf = conf;
		this.urlField = this.getConf().get(OutlinkMetaConfig.URL_FIELD);
		if (this.urlField == null) {
			String message = "Missing url field. Property should be set in config (" + OutlinkMetaConfig.URL_FIELD + ")\n";
			AbstractOutlinkMeta.logger.error(message);
			throw new RuntimeException(message);
		}
		this.urlDescription = this.getConf().get(OutlinkMetaConfig.URL_DESCRIPTION);
		if (this.urlDescription == null) {
			String message = "Missing url descrition. Property should be set in config (" + OutlinkMetaConfig.URL_DESCRIPTION + ")\n";
			AbstractOutlinkMeta.logger.error(message);
			throw new RuntimeException(message);
		}
		this.fields = this.getConf().getTrimmedStrings(OutlinkMetaConfig.FIELDS, new String[0]);
		this.indexBinary = this.getConf().getBoolean(OutlinkMetaConfig.INDEX_BINARY, false);
	}

}
