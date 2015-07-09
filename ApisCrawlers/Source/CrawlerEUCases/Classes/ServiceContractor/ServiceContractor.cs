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

namespace Crawler.Classes.ServiceContractor
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using Crawler.Classes.Common;
    using Crawler.ServiceEUCasesReference;
    using CrawlerEUCases.Data;
    using MetainfoEUCases;

    /// <summary>
    /// ServiceContractor class provides functionality to upload crawled document to WebService
    /// </summary>
    public class ServiceContractor : IDisposable
    {
        private bool disposed;

        private ServiceEUCasesClient webService;

        /// <summary>
        /// Initialize ServiceContractor
        /// </summary>
        public ServiceContractor()
        {
            this.webService = new ServiceEUCasesClient();
        }

        public void Dispose()
        {
            this.Dispose(true);

            // Call SupressFinalize in case a subclass implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Serialize information for crowled document to XMl and upload document and XML to Web Service
        /// </summary>
        /// <param name="documentGroupDb"></param>
        public bool UploadToService(DocumentGroup documentGroupDb)
        {
            try
            {
                string docPath = Arguments.DestinationFolder + "\\Data\\" + documentGroupDb.DocumentGroupName;
                var docArray = File.ReadAllBytes(docPath);

                UploadDocumentGroup uploadDocumentGroup = new UploadDocumentGroup();
                uploadDocumentGroup.Data = docArray;
                XmlDocumentGroup xmlDocumentGroup = this.GetNewXmlDocumentGroup(documentGroupDb);
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDocumentGroup));
                using (TextWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, xmlDocumentGroup);
                    uploadDocumentGroup.MetaInfo = writer.ToString();
                }

                string msg = this.webService.UploadFile(uploadDocumentGroup);

                if (msg != "Ok")
                {
                    CrawlerLog.LogInfo("Service Upload Error DocumentGroupId: " + documentGroupDb.DocumentGroupId + "Error:" + msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogException(ex);

                return false;
            }

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource. 
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.webService != null)
                    {
                        this.webService.Close();
                    }
                }

                this.webService = null;

                // Indicate that the instance has been disposed.
                this.disposed = true;
            }
        }

        private XmlDocumentGroup GetNewXmlDocumentGroup(DocumentGroup documentGroupDb)
        {
            XmlDocumentGroup xmlDocumentGroup = new XmlDocumentGroup();
            xmlDocumentGroup.Crawler = Arguments.CrawlerName;
            xmlDocumentGroup.Date = documentGroupDb.DocumentGroupDate;
            xmlDocumentGroup.FileName = documentGroupDb.DocumentGroupName;
            xmlDocumentGroup.Format = "application/zip";
            xmlDocumentGroup.Identifier = documentGroupDb.Identifier;
            xmlDocumentGroup.Lang = Arguments.Language;
            xmlDocumentGroup.Operation = (Operation)documentGroupDb.Operation;

            foreach (var documentDb in documentGroupDb.Documents)
            {
                XmlDocumentMetaInfo xmlDocumentMetaInfo = new XmlDocumentMetaInfo();
                xmlDocumentMetaInfo.File = documentDb.DocumentName;
                xmlDocumentMetaInfo.Format = documentDb.DocumentFormat;
                xmlDocumentMetaInfo.Identifier = documentDb.Identifier;
                xmlDocumentMetaInfo.Md5 = documentDb.Md5;
                xmlDocumentMetaInfo.Operation = (Operation)documentDb.Operation;
                xmlDocumentMetaInfo.Url = documentDb.Url;

                xmlDocumentGroup.Document.Add(xmlDocumentMetaInfo);
            }

            return xmlDocumentGroup;
        }
    }
}
