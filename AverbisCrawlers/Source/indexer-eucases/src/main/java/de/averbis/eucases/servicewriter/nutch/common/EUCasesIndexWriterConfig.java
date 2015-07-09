/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the EUPL V.1.1
 * https://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 *
 * @author Florian Schmedding
 *
 * Description: Configuration options for the indexer
 */

package de.averbis.eucases.servicewriter.nutch.common;

public interface EUCasesIndexWriterConfig {

	public static final String NAME = "eucases.servicewriter";

	public static final String SEPARATOR = ".";

	public static final String SERVICE_URL = EUCasesIndexWriterConfig.NAME + EUCasesIndexWriterConfig.SEPARATOR + "url";

	public static final String SEGMENTS_PATH = EUCasesIndexWriterConfig.NAME + EUCasesIndexWriterConfig.SEPARATOR + "segments";

	public static final String OUTPUT_PATH = EUCasesIndexWriterConfig.NAME + EUCasesIndexWriterConfig.SEPARATOR + "output";

}
