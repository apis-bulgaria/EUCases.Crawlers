/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * @author Florian Schmedding
 *
 * Description: TODO
 */

package de.averbis.eucases.outlinkmeta.nutch.scoring;

import java.util.Collection;
import java.util.List;
import java.util.Map.Entry;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.io.Text;
import org.apache.hadoop.io.Writable;
import org.apache.nutch.crawl.CrawlDatum;
import org.apache.nutch.crawl.Inlinks;
import org.apache.nutch.indexer.NutchDocument;
import org.apache.nutch.indexer.NutchField;
import org.apache.nutch.parse.Parse;
import org.apache.nutch.parse.ParseData;
import org.apache.nutch.protocol.Content;
import org.apache.nutch.scoring.ScoringFilter;
import org.apache.nutch.scoring.ScoringFilterException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import de.averbis.eucases.outlinkmeta.nutch.common.AbstractOutlinkMeta;

public class OutlinkMetaScoringFilter extends AbstractOutlinkMeta implements ScoringFilter {

	private final static Logger logger = LoggerFactory.getLogger(OutlinkMetaScoringFilter.class);


	public OutlinkMetaScoringFilter() {

		super();
	}


	/**
	 * This method is not used in this plugin and does nothing.
	 * 
	 * @param url
	 * @param datum
	 * @throws ScoringFilterException
	 */
	@Override
	public void injectedScore(Text url, CrawlDatum datum) throws ScoringFilterException {

		return;
	}


	/**
	 * If the CrawlDatum datum contains the metadata field OutlinkMetaConfig.URL_FIELD the value of this field is updated to the value of url because it is the normalized URL for
	 * the document. In contrast, the URL currently contained in the metadata can be different.
	 * 
	 * Otherwise the metadata cannot be updated in updateDbScore() because the URLs will not necessarily match.
	 * 
	 * Example: A wrong url like http://example.org//index/test.pdf gets normalized to http://example.org/index/test.pdf
	 * 
	 * @param url
	 * @param datum
	 * @throws ScoringFilterException
	 */
	@Override
	public void initialScore(Text url, CrawlDatum datum) throws ScoringFilterException {

		Writable outlinkWritable = datum.getMetaData().get(new Text(this.getUrlField()));
		if (outlinkWritable == null) {
			// If the field does not exist or has value null just continue
			return;
		}
		if (!(outlinkWritable instanceof NutchField)) {
			// All fields added by outlinkmeta have type NutchField
			OutlinkMetaScoringFilter.logger.warn("Field {} should have type NutchField but has type {}", this.getUrlField(), outlinkWritable.getClass().getCanonicalName());
			return;
		}

		// set the outlink to the received normalized url of the CrawlDatum
		NutchField outlink = (NutchField) outlinkWritable;
		outlink.reset();
		outlink.add(url.toString());

		return;
	}


	/**
	 * This method is not used in this plugin and returns the given initSort value.
	 * 
	 * @param url
	 * @param datum
	 * @param initSort
	 * @return unchanged initSort value
	 * @throws ScoringFilterException
	 */
	@Override
	public float generatorSortValue(Text url, CrawlDatum datum, float initSort) throws ScoringFilterException {

		return initSort;
	}


	/**
	 * All fields from the outlinkmeta configuration are made available to the parser by putting them into the content metadata. The metadata are retrieved from the CrawlDatum so
	 * this happens only for documents where the corresponding outlink was created by the outlinkmeta plugin.
	 * 
	 * @param url
	 * @param datum
	 * @param content
	 * @throws ScoringFilterException
	 */
	@Override
	public void passScoreBeforeParsing(Text url, CrawlDatum datum, Content content) throws ScoringFilterException {

		for (String field : this.getFields()) {
			Object value = datum.getMetaData().get(new Text(field));

			// All fields added by outlinkmeta have type NutchField
			if (value instanceof NutchField) {
				NutchField metadata = (NutchField) value;
				for (Object val : metadata.getValues()) {
					content.getMetadata().add(field, val.toString());
				}
			} else if (value != null) {
				OutlinkMetaScoringFilter.logger.warn("Field {} should have type NutchField but has type {}", field, value.getClass().getCanonicalName());
			}
			// if value is null do not add it so nothing more to do here
		}
		return;
	}


	/**
	 * This method is not used in this plugin and does nothing.
	 * 
	 * @param url
	 * @param content
	 * @param parse
	 * @throws ScoringFilterException
	 */
	@Override
	public void passScoreAfterParsing(Text url, Content content, Parse parse) throws ScoringFilterException {

		return;
	}


	/**
	 * This method is not used in this plugin and does nothing.
	 * 
	 * @param fromUrl
	 * @param parseData
	 * @param targets
	 * @param adjust
	 * @param allCount
	 * @return
	 * @throws ScoringFilterException
	 */
	@Override
	public CrawlDatum distributeScoreToOutlinks(Text fromUrl, ParseData parseData, Collection<Entry<Text, CrawlDatum>> targets, CrawlDatum adjust, int allCount)
			throws ScoringFilterException {

		return adjust;
	}


	/**
	 * Search for metadata about the CrawlDatum datum in the inlinks. If meadata is found it is added to datum and the fetch time of datum is set to now in order to trigger an
	 * re-fetch.
	 * 
	 * @param url
	 * @param old
	 * @param datum
	 *            (metadata and fetch time might be changed)
	 * @param inlinked
	 * @throws ScoringFilterException
	 */
	@Override
	public void updateDbScore(Text url, CrawlDatum old, CrawlDatum datum, List<CrawlDatum> inlinked) throws ScoringFilterException {

		for (CrawlDatum inlink : inlinked) {

			Writable outlinkWritable = inlink.getMetaData().get(new Text(this.getUrlField()));
			if (outlinkWritable == null) {
				// If the field does not exist or has value null just continue
				continue;
			}
			if (!(outlinkWritable instanceof NutchField)) {
				// All fields added by outlinkmeta have type NutchField
				OutlinkMetaScoringFilter.logger.warn("Field {} should have type NutchField but has type {}", this.getUrlField(), outlinkWritable.getClass().getCanonicalName());
				continue;
			}

			// outlinkmeta does never add an empty NutchField
			String outlink = ((NutchField) outlinkWritable).getValues().get(0).toString();

			if (url.toString().equals(outlink)) {
				// the inlink contains metadata about the current CrawlDatum
				for (String field : this.getFields()) {
					Writable value = inlink.getMetaData().get(new Text(field));
					if (value == null) {
						// If value is null the field does not exist in the metadata (outlinkmeta does never add null values).
						// Null values (and keys) must never be added to Mapwritable because it calls getClass() on it.
						continue;
					}
					datum.getMetaData().put(new Text(field), value);
				}
				datum.setFetchTime(System.currentTimeMillis());
				// the following is not necessary to trigger the fetch
				// datum.setStatus(CrawlDatum.STATUS_DB_UNFETCHED);
				// datum.setRetriesSinceFetch(0);
				// datum.setSignature(null);
				// datum.setModifiedTime(0L);
			}

		}

		return;
	}


	/**
	 * This method is not used in this plugin and returns the given initSort value.
	 * 
	 * @param url
	 * @param doc
	 * @param dbDatum
	 * @param fetchDatum
	 * @param parse
	 * @param inlinks
	 * @param initScore
	 * @return unchanged initSort value
	 * @throws ScoringFilterException
	 */
	@Override
	public float indexerScore(Text url, NutchDocument doc, CrawlDatum dbDatum, CrawlDatum fetchDatum, Parse parse, Inlinks inlinks, float initScore) throws ScoringFilterException {

		return initScore;
	}


	@Override
	public void setConf(Configuration conf) {

		super.setConf(conf);

	}


	@Override
	public Configuration getConf() {

		return super.getConf();
	}

}
