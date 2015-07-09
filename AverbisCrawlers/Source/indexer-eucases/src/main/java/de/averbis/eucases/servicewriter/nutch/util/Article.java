/**
 * Copyright (c) 2014, Averbis GmbH. All rights reserved.
 *
 * Licensed under the EUPL V.1.1
 * https://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 *
 * @author Florian Schmedding
 *
 * Description: Utility class for handling the article metadata.
 */

package de.averbis.eucases.servicewriter.nutch.util;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;
import java.io.StringReader;
import java.io.StringWriter;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.LinkedList;
import java.util.List;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.Unmarshaller;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlTransient;

import org.apache.commons.codec.binary.Base64;
import org.apache.commons.io.FileUtils;
import org.apache.commons.io.IOUtils;
import org.apache.nutch.indexer.NutchDocument;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

@XmlRootElement(name = "article")
@XmlAccessorType(XmlAccessType.FIELD)
public class Article implements Serializable {

	/**
	 * 
	 */
	private static final long serialVersionUID = -1432358835771663161L;

	private final static Logger logger = LoggerFactory.getLogger(Article.class);

	@XmlElement
	private String title;

	@XmlElementWrapper(name = "authors")
	@XmlElement(name = "name")
	private final List<String> authors = new LinkedList<String>();

	@XmlElement(name = "date")
	private String publicationDate;

	/**
	 * The url of the retrieved document coming from Nutch
	 */
	@XmlElement
	private String id;

	/**
	 * The mimetype of the retrieved document coming from Nutch
	 */
	@XmlElement
	private String mimetype;

	/**
	 * The content hash from Nutch
	 */
	@XmlElement
	private String digest;

	/**
	 * The segment where Nutch has stored the content
	 */
	@XmlElement
	private String segment;

	/**
	 * The url of the article found in the article overview
	 */
	@XmlElement
	private String url;

	/**
	 * The title of the journal where the article is published
	 */
	@XmlElement
	private String journal;

	/**
	 * The url of the journal
	 */
	@XmlElement
	private String source;

	/**
	 * The binary content of the article
	 */
	@XmlTransient
	private byte[] content;


	public Article() {

		super();
	}


	public Article(String title) {

		this();
		this.title = title;
	}


	public Article(NutchDocument doc) {

		if (doc.getFieldValue(ArticleFields.ID) != null) {
			this.id = doc.getFieldValue(ArticleFields.ID).toString();
		}

		if (doc.getFieldValue(ArticleFields.DIGEST) != null) {
			this.digest = doc.getFieldValue(ArticleFields.DIGEST).toString();
		}

		if (doc.getFieldValue(ArticleFields.SEGMENT) != null) {
			this.segment = doc.getFieldValue(ArticleFields.SEGMENT).toString();
		}

		if (doc.getFieldValue(ArticleFields.URL) != null) {
			this.url = doc.getFieldValue(ArticleFields.URL).toString();
		}

		if (doc.getFieldValue(ArticleFields.TYPE) != null) {
			this.mimetype = doc.getFieldValue(ArticleFields.TYPE).toString();
		}

		if (doc.getFieldValue(ArticleFields.BINARY) != null) {
			this.content = Base64.decodeBase64(doc.getFieldValue(ArticleFields.BINARY).toString());
		}

		if (doc.getFieldValue(ArticleFields.TITLE) != null) {
			this.title = doc.getFieldValue(ArticleFields.TITLE).toString();
		}

		if (doc.getFieldValue(ArticleFields.JOURNAL) != null) {
			this.journal = doc.getFieldValue(ArticleFields.JOURNAL).toString();
		}

		if (doc.getFieldValue(ArticleFields.SOURCE) != null) {
			this.source = doc.getFieldValue(ArticleFields.SOURCE).toString();
		}

		if (doc.getField(ArticleFields.AUTHORS) != null) {
			for (Object val : doc.getField(ArticleFields.AUTHORS).getValues()) {
				this.addAuthor(val.toString());
			}
		}

		if (doc.getFieldValue(ArticleFields.DATE) != null) {
			try {
				this.setPublicationDate(doc.getFieldValue(ArticleFields.DATE).toString());
			} catch (ParseException e) {
				String date = doc.getFieldValue(ArticleFields.DATE).toString();
				Article.logger.warn("Date format is wrong in {}", date);
			}
		}
	}


	public void addAuthor(String author) {

		this.authors.add(author);
	}


	public List<String> getAuthors() {

		return this.authors;
	}


	public String getTitle() {

		return this.title;
	}


	public void setTitle(String title) {

		this.title = title;
	}


	/**
	 * Returns the publication date in Solr date format (UTC)
	 * 
	 * @return the publication date formatted as yyyy-MM-ddT00:00:00Z or {@code null} if the date is not set
	 */
	public String getPublicationDate() {

		if (this.publicationDate == null) {
			return null;
		}
		return this.publicationDate + "T00:00:00Z";
	}


	/**
	 * Convenience method for setting the publication date. The given date will be converted immediately to a string with format yyyy-MM-dd.
	 * 
	 * @param publicationDate
	 */
	public void setPublicationDate(Date publicationDate) {

		this.publicationDate = new SimpleDateFormat("yyyy-MM-dd").format(publicationDate);
	}


	/**
	 * Set the publication date. If the string does not have the required format a warning is logged but nothing is done.
	 * 
	 * @param publicationDate
	 *            Date with format yyyy-MM-dd
	 */
	public void setPublicationDate(String publicationDate) throws ParseException {

		try {
			Date date = new SimpleDateFormat("yyyy-MM-dd").parse(publicationDate);
			this.setPublicationDate(date);
		} catch (ParseException e) {
			Article.logger.warn("Could not set publication date {}.", publicationDate);
		}
	}


	public String getId() {

		return this.id;
	}


	public void setId(String id) {

		this.id = id;
	}


	public String getDigest() {

		return this.digest;
	}


	public void setDigest(String digest) {

		this.digest = digest;
	}


	public String getSegment() {

		return this.segment;
	}


	public void setSegment(String segment) {

		this.segment = segment;
	}


	public String getUrl() {

		return this.url;
	}


	public void setUrl(String url) {

		this.url = url;
	}


	public String getMimetype() {

		return this.mimetype;
	}


	public void setMimetype(String mimetype) {

		this.mimetype = mimetype;
	}


	public String getFileExtension() {

		if ("application/xhtml+xml".equals(this.mimetype)) {
			return "html";
		}
		if ("text/html".equals(this.mimetype)) {
			return "html";
		}
		if ("application/xml".equals(this.mimetype)) {
			return "xml";
		}
		if ("application/pdf".equals(this.mimetype)) {
			return "pdf";
		}
		return "bin";
	}


	public String getJournal() {

		return this.journal;
	}


	public void setJournal(String journal) {

		this.journal = journal;
	}


	public String getSource() {

		return this.source;
	}


	public void setSource(String source) {

		this.source = source;
	}


	public byte[] getContent() {

		return this.content;
	}


	public void setContent(byte[] content) {

		this.content = content;
	}


	public boolean isUseful() {

		if (this.url == null) {
			// Article.logger.error("Article does not have url.");
			return false;
		}

		// if (this.segment == null) {
		// Article.logger.error("Article {} does not have segment information.", this.url);
		// return false;
		// }

		if (this.digest == null) {
			// Article.logger.error("Article {} does not have digest information.", this.url);
			return false;
		}

		if (this.content == null) {
			// Article.logger.error("Article {} does not have binary content.", this.url);
			return false;
		}

		return true;
	}


	@Override
	public String toString() {

		return String.format(
				"Id: %s; Title: %s; Publication date: %s",
				this.id,
				this.title,
				this.publicationDate);
	}


	/**
	 * Write the instance to a Base64 string.
	 * 
	 * @return Base64-encoded serialization of the instance or {@code null} if the serialization was not successful.
	 */
	public String serialize() {

		ObjectOutputStream oos = null;
		try {
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			oos = new ObjectOutputStream(baos);
			oos.writeObject(this);
			return Base64.encodeBase64String(baos.toByteArray());
		} catch (Exception e) {
			return null;
		} finally {
			IOUtils.closeQuietly(oos);
		}
	}


	/**
	 * Read an instance from a Base64 string.
	 * 
	 * @param base64
	 *            The base64-encoded object serialization of an {@link Article} instance.
	 * @return A {@link Article} instance or {@code null} if the deserialization was not successful.
	 */
	public static Article deserialize(String base64) {

		byte[] data = Base64.decodeBase64(base64);
		ObjectInputStream ois = null;
		try {
			ois = new ObjectInputStream(new ByteArrayInputStream(data));
			Article context = (Article) ois.readObject();
			return context;
		} catch (IOException e) {
			Article.logger.error("Could not create journal link context from base64.");
			return null;
		} catch (ClassNotFoundException e) {
			Article.logger.error("Could not create journal link context from base64.");
			return null;
		} finally {

			IOUtils.closeQuietly(ois);
		}
	}


	/**
	 * Write the instance to a Xml string.
	 * 
	 * @return Xml-encoded serialization of the instance or {@code null} if the serialization was not successful.
	 */
	public String toXml() {

		try {
			JAXBContext jc = JAXBContext.newInstance(Article.class);
			Marshaller marshaller = jc.createMarshaller();
			marshaller.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, false);
			StringWriter xml = new StringWriter();
			marshaller.marshal(this, xml);
			return xml.toString();
		} catch (JAXBException e) {
			Article.logger.error("Could not serialize journal link context {}", this.toString());
			return null;
		}
	}


	/**
	 * Read an instance from a Xml string.
	 * 
	 * @param xml
	 *            The xml-encoded object serialization of an {@link Article} instance.
	 * @return A {@link Article} instance or {@code null} if the deserialization was not successful.
	 */
	public static Article fromXml(String xml) {

		try {
			JAXBContext jc = JAXBContext.newInstance(Article.class);
			Unmarshaller jaxbUnmarshaller = jc.createUnmarshaller();
			Article context = (Article) jaxbUnmarshaller.unmarshal(new StringReader(xml));
			return context;
		} catch (JAXBException e) {
			Article.logger.error("Could not create journal link context from xml.");
			return null;
		}
	}


	public void writeXmlFile(String path, String filename) throws IOException {

		FileUtils.write(new File(path, filename), this.toXml(), "UTF-8");
	}
}
