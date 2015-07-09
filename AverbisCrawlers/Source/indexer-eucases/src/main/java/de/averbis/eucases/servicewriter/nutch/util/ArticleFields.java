/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the EUPL V.1.1
 * https://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 *
 * @author Florian Schmedding
 *
 * Description: Constants for the article metadata keys.
 */

package de.averbis.eucases.servicewriter.nutch.util;

public interface ArticleFields { // extends Nutch {

	public static final String TITLE = "eucases.article-title";
	public static final String AUTHORS = "eucases.article-authors";
	public static final String DATE = "eucases.article-date";
	public static final String URL = "eucases.article-url";
	public static final String JOURNAL = "eucases.journal";
	public static final String SOURCE = "eucases.source";

	public static final String BINARY = "outlinkmeta.base64";
	// public static final String BINARY = de.averbis.eucases.outlinkmeta.nutch.common.OutlinkMetaConfig.BINARY_CONTENT

	// The url field comes from the index-basic plugin (no constant available). It contains the document url.
	public static final String ID = "url";
	// The type field comes from the index-more plugin (no constant available). It contains the complete mimetype, the mediatype, and the subtype.
	public static final String TYPE = "type";
	public static final String DIGEST = "digest";
	public static final String SEGMENT = "segment";
	// public static final String DIGEST = Nutch.SIGNATURE_KEY;
	// public static final String SEGMENT = Nutch.SEGMENT_NAME_KEY;

}
