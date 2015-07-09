/* 
 * Copyright 2014 Apis Hristovich EOOD.
 *
 * Licensed under the European Union Public Licence (EUPL), Version 1.1 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://joinup.ec.europa.eu/software/page/eupl/licence-eupl
 *
 * Unless required by applicable law, software distributed under the License is 
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing 
 * permissions and limitations under the License.
 */

namespace ServiceEUCases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Classes;
    using Data;
    using Ionic.Zip;
    using MetainfoEUCases;
    using Repositories;

    /// <summary>
    /// Implements IServiceEUCases and do all job for checking validity of XmlDocumentGroup
    /// and store in database
    /// </summary>
    public class ServiceEUCases : IServiceEUCases
    {
        private bool isValidXml = true;
        private StringBuilder validationErrorLog = new StringBuilder();
        private IRepository db;

        public ServiceEUCases()
        {
            this.db = new DbRepository();
        }

        public ServiceEUCases(IRepository repository)
        {
            this.db = repository;
        }

        /// <summary>
        /// Method that is used to upload XmlDocumentGroup to web service
        /// </summary>
        /// <param name="uploadDocumentGroup">Complex type XmlDocumentGroup</param>
        /// <returns>- If there is no errors the return string is "Ok" 
        /// - If error the return string will be in following format: 
        /// Error: <error description>
        /// </returns>
        public string UploadFile(UploadDocumentGroup uploadDocumentGroup)
        {
            CrawlerLog crawlerLog = new CrawlerLog();
            try
            {
                RemoteEndpointMessageProperty msg = null;
                if (OperationContext.Current != null)
                {
                    msg = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    crawlerLog.IpAddress = msg.Address;
                }
                else
                {
                    crawlerLog.IpAddress = "localhost";
                }

                crawlerLog.IsSuccess = false;
                crawlerLog.LogDate = DateTime.Now;
                crawlerLog.MetaXml = uploadDocumentGroup.MetaInfo;

                // ValidateDeserializeXml
                this.isValidXml = true;
                this.validationErrorLog.Clear();
                this.XmlValidate(uploadDocumentGroup.MetaInfo);
                if (!this.isValidXml)
                {
                    return this.validationErrorLog.ToString();
                }

                XmlDocumentGroup xmlDocumentGroup = null;
                XmlSerializer deserializer = new XmlSerializer(typeof(XmlDocumentGroup));
                using (TextReader textReader = new StringReader(uploadDocumentGroup.MetaInfo))
                {
                    xmlDocumentGroup = (XmlDocumentGroup)deserializer.Deserialize(textReader);
                }

                crawlerLog.Identifier = xmlDocumentGroup.Identifier;
                crawlerLog.Operation = (int)xmlDocumentGroup.Operation;
                crawlerLog.CrawlerName = xmlDocumentGroup.Crawler;

                // ValidateReadZip
                Dictionary<string, MemoryStream> documentsData = new Dictionary<string, MemoryStream>();
                if (xmlDocumentGroup.Operation != Operation.Del)
                {
                    if (uploadDocumentGroup.Data == null || uploadDocumentGroup.Data.Length == 0)
                    {
                        this.validationErrorLog.Append("Error: Validation Exception - ZipData length is zero or ZipData is NULL, document idenfier: " + xmlDocumentGroup.Identifier + " and Operation: " + xmlDocumentGroup.Operation);
                        return this.validationErrorLog.ToString();
                    }
                    else
                    {
                        crawlerLog.ZipLength = uploadDocumentGroup.Data.Length;
                        documentsData = this.UnZipToMemory(uploadDocumentGroup.Data);
                    }
                }

                // AddUpdDel
                var documentGroupIdentifier = this.db.GetIdentifier(xmlDocumentGroup.Identifier);

                if (xmlDocumentGroup.Operation == Operation.Add)
                {
                    if (string.IsNullOrEmpty(documentGroupIdentifier))
                    {
                        // Add new XmlDocumentGroup
                        this.AddNewDocumentGroup(xmlDocumentGroup, uploadDocumentGroup, documentsData);
                    }
                    else
                    {
                        this.validationErrorLog.Append("Error: Validation Exception - There is a document identifier: " + xmlDocumentGroup.Identifier + " You can't use Оperation: " + xmlDocumentGroup.Operation);
                        return this.validationErrorLog.ToString();
                    }
                }
                else if (xmlDocumentGroup.Operation == Operation.Upd || xmlDocumentGroup.Operation == Operation.Del)
                {
                    if (!string.IsNullOrEmpty(documentGroupIdentifier))
                    {
                        // Update/Delete
                        this.UpdateDeleteDocumentGroup(xmlDocumentGroup, uploadDocumentGroup, documentsData, documentGroupIdentifier);
                    }
                    else
                    {
                        this.validationErrorLog.Append("Error: Validation Exception - No document idenfier: " + xmlDocumentGroup.Identifier + " .You can't use Operation: " + xmlDocumentGroup.Operation);
                        return this.validationErrorLog.ToString();
                    }
                }
                else
                {
                    this.validationErrorLog.Append("Error: Validation Exception - Document idenfier: " + xmlDocumentGroup.Identifier + " .Invalid Operation: " + xmlDocumentGroup.Operation);
                    return this.validationErrorLog.ToString();
                }

                crawlerLog.IsSuccess = true;
            }
            catch (Exception ex)
            {
                crawlerLog.IsSuccess = false;
                crawlerLog.Error += ex.ToString();
                return "Error: Service Exception - " + ex.ToString();
            }
            finally
            {
                crawlerLog.Error += this.validationErrorLog.ToString();

                try
                {
                    this.db.AddNewLog(crawlerLog);
                    LockHelper.PushLocalLogToDb();
                }
                catch (Exception ex)
                {
                    // There is a problem with the connection to the database, so we recorded a log in a text file
                    crawlerLog.Error += ex.ToString();
                    LockHelper.WriteToFileLog(crawlerLog);
                }
            }

            return "Ok";
        }

        #region XmlValidation
        private void XmlValidate(string metaInfo)
        {
            // Read uploaded xml
            string indexStr = "<documentgroup ";
            string xmlString = metaInfo.Insert(metaInfo.IndexOf(indexStr) + indexStr.Length, " xmlns='xsdDocumentgroup' ");
            TextReader textReaderXml = new StringReader(xmlString);
            System.Xml.XmlTextReader trnodeXml = new System.Xml.XmlTextReader(textReaderXml);
            System.Xml.XmlValidatingReader vr = new System.Xml.XmlValidatingReader(trnodeXml);

            // Read xsd
            TextReader textReaderXsd = new StringReader(XsdSchema.XsdDocumentGroup);
            System.Xml.XmlReader trnodeXsd = new System.Xml.XmlTextReader(textReaderXsd);

            vr.ValidationType = System.Xml.ValidationType.Schema;
            vr.Schemas.Add(null, trnodeXsd);
            vr.ValidationEventHandler += new ValidationEventHandler(this.ValidationHandler);

            while (vr.Read())
            {
            }
        }

        private void ValidationHandler(object sender, ValidationEventArgs args)
        {
            this.isValidXml = false;
            this.validationErrorLog.AppendLine("Error: Validation Exception - " + args.Message);
        }
        #endregion

        #region MemoryUnzip
        private Dictionary<string, MemoryStream> UnZipToMemory(byte[] zipData)
        {
            Stream stream = new MemoryStream(zipData, 0, zipData.Length);
            var result = new Dictionary<string, MemoryStream>();
            using (ZipFile zip = ZipFile.Read(stream))
            {
                foreach (ZipEntry e in zip)
                {
                    MemoryStream data = new MemoryStream();
                    e.Extract(data);
                    result.Add(e.FileName, data);
                }
            }

            return result;
        }
        #endregion

        #region UpdAddDel
        private void UpdateDeleteDocumentGroup(
            XmlDocumentGroup xmlDocumentGroup,
            UploadDocumentGroup uploadDocumentGroup,
            Dictionary<string, MemoryStream> documentsData,
            string documentGroupIdentifier)
        {
            // Delete
            this.db.DeleteDocumentGroup(documentGroupIdentifier);

            // Update
            if (xmlDocumentGroup.Operation == Operation.Upd)
            {
                this.AddNewDocumentGroup(xmlDocumentGroup, uploadDocumentGroup, documentsData);
            }
        }

        private void AddNewDocumentGroup(
            XmlDocumentGroup xmlDocumentGroup,
            UploadDocumentGroup uploadDocumentGroup,
            Dictionary<string, MemoryStream> documentsData)
        {
            DocumentGroup newDocumentGroup = new DocumentGroup();
            newDocumentGroup.CrawlerId = this.db.GetOrCreateCrawlerId(xmlDocumentGroup.Crawler);
            newDocumentGroup.Identifier = xmlDocumentGroup.Identifier;
            newDocumentGroup.DocumentGroupName = xmlDocumentGroup.FileName;
            newDocumentGroup.DocumentGroupFormat = xmlDocumentGroup.Format;
            newDocumentGroup.Lang = xmlDocumentGroup.Lang;
            newDocumentGroup.Operation = (int)xmlDocumentGroup.Operation;
            newDocumentGroup.DocumentGroupDate = xmlDocumentGroup.Date;

            // Byte Array Data
            newDocumentGroup.DataContent = uploadDocumentGroup.Data;

            List<Document> documents = this.AddDocuments(xmlDocumentGroup.Document, documentsData);
            foreach (Document document in documents)
            {
                newDocumentGroup.Documents.Add(document);
            }

            this.db.AddDocumentGroup(newDocumentGroup);
        }

        private List<Document> AddDocuments(List<XmlDocumentMetaInfo> xmlDocuments, Dictionary<string, MemoryStream> documentsData)
        {
            List<Document> documents = new List<Document>();
            for (int i = 0; i < xmlDocuments.Count; i++)
            {
                Document document = new Document();
                document.DocumentName = xmlDocuments[i].File;
                document.Identifier = xmlDocuments[i].Identifier;
                document.DocumentFormat = xmlDocuments[i].Format;
                document.Operation = (int)xmlDocuments[i].Operation;
                document.DocumentOrder = i;
                document.Url = xmlDocuments[i].Url;
                document.Md5 = xmlDocuments[i].Md5;
                var docData = documentsData[xmlDocuments[i].File];
                if (docData != null)
                {
                    document.DataContent = docData.ToArray();
                }

                documents.Add(document);
            }

            return documents;
        }
        #endregion
    }
}
