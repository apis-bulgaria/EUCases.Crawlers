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
 * Description: Configuration parameters for the plugin.
 */

package de.averbis.eucases.outlinkmeta.nutch.common;

public interface OutlinkMetaConfig {

	public static final String NAME = "outlinkmeta";

	public static final String SEPARATOR = ".";

	public static final String URL_FIELD = OutlinkMetaConfig.NAME + OutlinkMetaConfig.SEPARATOR + "urlField";

	public static final String URL_DESCRIPTION = OutlinkMetaConfig.NAME + OutlinkMetaConfig.SEPARATOR + "urlDescription";

	public static final String FIELDS = OutlinkMetaConfig.NAME + OutlinkMetaConfig.SEPARATOR + "fields";

	public static final String INDEX_BINARY = OutlinkMetaConfig.NAME + OutlinkMetaConfig.SEPARATOR + "indexBinary";

	public static final String BINARY_CONTENT = OutlinkMetaConfig.NAME + OutlinkMetaConfig.SEPARATOR + "base64";
}
