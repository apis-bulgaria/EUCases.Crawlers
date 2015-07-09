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

namespace Crawler.Classes.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Crawler.Classes.CRCompareXml;
    using Crawler.Classes.ServiceContractor;
    using CrawlerEUCases.Data;
    using ICSharpCode.SharpZipLib.Zip;
    using MetainfoEUCases;

    /// <summary>
    /// CrawlerManager is responsible for whole crawling process and uploading of documents to the Web Service
    /// </summary>
    public class CrawlerManager
    {
        private static readonly int EtityWriteSize = 2000;
        private static readonly int EntityUploadSize = 2000;
        private object objectLock;
        private SQLiteCrawlerDataEntities context;
        private int counter;

        /// <summary>
        /// Initializes a new instance of the CrawlerManager class.
        /// </summary>
        public CrawlerManager()
        {
            this.objectLock = new object();

            this.InitializeFolders();
            this.InitializeDatabase();
            this.UploadToService();
        }

        /// <summary>
        /// For each downloaded document start the process of checking the differences. 
        /// Save and send the document to the service, if there is a differences.
        /// </summary>
        /// <param name="crawlerClass">Accept BaseCrawler <see cref="BaseCrawler"/> class.</param>
        public void Start(BaseCrawler crawlerClass)
        {
            this.counter = 0;
            this.context = new SQLiteCrawlerDataEntities(Arguments.SQLiteConnectionString);
            this.context.Configuration.ValidateOnSaveEnabled = false;

            crawlerClass.OnDocDownloaded += this.CrawlerClass_OnDocDownloaded;
            crawlerClass.Start();

            if (this.counter > 0)
            {
                try
                {
                    this.context.SaveChanges();
                }
                catch (Exception ex)
                {
                    CrawlerLog.LogException(ex);
                }
            }

            this.UploadToService();
        }

        /// <summary>
        /// Check if there is already downloaded documents from current Crawler.
        /// If there are't create new Database.
        /// </summary>
        private void InitializeDatabase()
        {
            if (!File.Exists(Arguments.SQLiteDataFile))
            {
                File.Copy(".\\SQLiteData\\SQLiteCrawlerData.sqlite", Arguments.SQLiteDataFile);
            }
        }

        /// <summary>
        /// Check if there are existing directories 
        /// Data and MetaInfo and if there are not creates new one.
        /// </summary>
        private void InitializeFolders()
        {
            if (!Directory.Exists(Arguments.DestinationFolder + "\\SQLiteData"))
            {
                Directory.CreateDirectory(Arguments.DestinationFolder + "\\SQLiteData");
            }

            if (!Directory.Exists(Arguments.DestinationFolder + "\\Data"))
            {
                Directory.CreateDirectory(Arguments.DestinationFolder + "\\Data");
            }
        }

        private void UpdateSqLiteDB(List<long> docGroupIds)
        {
            string connectionString = "data source=" + Arguments.SQLiteDataFile;
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                con.Open();
                using (System.Data.SQLite.SQLiteTransaction tran = con.BeginTransaction())
                {
                    using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                    {
                        foreach (int id in docGroupIds)
                        {
                            com.CommandText = " UPDATE DocumentGroups SET Operation=3 WHERE DocumentGroupId = " + id;
                            com.ExecuteNonQuery();

                            com.CommandText = " UPDATE Documents SET Operation=3 WHERE DocumentGroupId = " + id;
                            com.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                }
            }
        }

        /// <summary>
        /// For each document group sent to the service metinfo 
        /// for each document together with his data represented such as byte array.
        /// </summary>
        private void UploadToService()
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                // fetch all documentgroup names for upload
                List<string> docGroupNames;
                using (var context = new SQLiteCrawlerDataEntities(Arguments.SQLiteConnectionString))
                {
                    docGroupNames = (from dg in context.DocumentGroups
                                     where dg.Operation != (int)Operation.None
                                     select dg.DocumentGroupName).ToList();
                }

                List<long> docGroupIds = new List<long>(EntityUploadSize);

                Parallel.ForEach(
                      docGroupNames,
                      new ParallelOptions { MaxDegreeOfParallelism = 5 },
                      docGroupName =>
                      {
                          using (var context = new SQLiteCrawlerDataEntities(Arguments.SQLiteConnectionString))
                          {
                              DocumentGroup documentGroup = (from d in context.DocumentGroups.Include("Documents")
                                                             where d.DocumentGroupName == docGroupName
                                                             select d).FirstOrDefault();
                              using (ServiceContractor sc = new ServiceContractor())
                              {
                                  bool isNormalUpload = sc.UploadToService(documentGroup);

                                  if (isNormalUpload)
                                  {
                                      lock (objectLock)
                                      {
                                          docGroupIds.Add(documentGroup.DocumentGroupId);
                                          if (docGroupIds.Count() > EntityUploadSize)
                                          {
                                              UpdateSqLiteDB(docGroupIds);

                                              docGroupIds = new List<long>(EntityUploadSize);
                                          }
                                      }
                                  }
                              }
                          }
                      });

                this.UpdateSqLiteDB(docGroupIds);
            }
            finally
            {
                sw.Stop();
            }

            CrawlerLog.LogInfo("upload time in ms:" + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Take a decision whether a document is to update, add or delete. 
        /// The decision is based on the document MetaInfoto on each document and the differences in MD5 sum.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrawlerClass_OnDocDownloaded(object sender, EventArgs e)
        {
            var filesMetaInfo = (e as DocDownloadedEventArguments).Document.ToList();

            string folderName = (e as DocDownloadedEventArguments).Folder.ToLower();
            string zipFileName = folderName + ".zip";
            string docDir = Path.Combine(Arguments.DataFolder, folderName);
            bool isProcess = false;

            DocumentGroup documentGroupDb = (from d in this.context.DocumentGroups.Include("Documents")
                                             where d.DocumentGroupName == zipFileName
                                             select d).FirstOrDefault();

            if (documentGroupDb != null)
            {
                // Upd DocumentGroup
                isProcess = this.UpdateDocumentGroup(filesMetaInfo, documentGroupDb);
            }
            else
            {
                // Add DocumentGroup
                isProcess = this.AddDocumentGroup(filesMetaInfo, zipFileName, this.context);
            }

            // Del???
            if (isProcess)
            {
                // Create zip
                this.MemoryZip(docDir, filesMetaInfo);
                this.counter++;

                // Add/Upd this DocumentGroups in database
                if (this.counter > EtityWriteSize)
                {
                    try
                    {
                        this.context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        CrawlerLog.LogException(ex);
                    }

                    this.context.Dispose();
                    this.context = new SQLiteCrawlerDataEntities(Arguments.SQLiteConnectionString);
                    this.context.Configuration.ValidateOnSaveEnabled = false;
                    this.counter = 0;
                }
            }
        }

        /// <summary>
        /// Add new DocumentGroup
        /// </summary>
        /// <param name="filesMetaInfo"></param>
        /// <param name="docDir"></param>
        /// <param name="zipFileName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool AddDocumentGroup(
            List<XmlDocumentMetaInfo> filesMetaInfo,
            string zipFileName,
            SQLiteCrawlerDataEntities context)
        {
            bool isProcess = false;

            // Add DocumentGroup
            DocumentGroup newDocumentGroup = new DocumentGroup();
            newDocumentGroup.DocumentGroupDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            newDocumentGroup.DocumentGroupName = zipFileName.ToLower();
            newDocumentGroup.Identifier = Guid.NewGuid().ToString();
            newDocumentGroup.Operation = (int)Operation.Add;

            foreach (var fileMetaInfo in filesMetaInfo)
            {
                fileMetaInfo.Md5 = MD5HashHelper.GetMd5Hash(fileMetaInfo.DataContent);
                fileMetaInfo.Operation = Operation.Add;
                fileMetaInfo.Identifier = Guid.NewGuid().ToString();
                isProcess = true;

                Document document = this.ReturnNewDocument(fileMetaInfo);
                newDocumentGroup.Documents.Add(document);
            }

            context.DocumentGroups.Add(newDocumentGroup);

            return isProcess;
        }

        /// <summary>
        /// Update an existing DocumentGroup
        /// </summary>
        /// <param name="filesMetaInfo"></param>
        /// <param name="docDir"></param>
        /// <param name="documentGroupDb"></param>
        /// <returns></returns>
        private bool UpdateDocumentGroup(
            List<XmlDocumentMetaInfo> filesMetaInfo,
            DocumentGroup documentGroupDb)
        {
            bool isProcess = false;

            foreach (var fileMetaInfo in filesMetaInfo)
            {
                fileMetaInfo.Md5 = MD5HashHelper.GetMd5Hash(fileMetaInfo.DataContent);
                var documentDb = documentGroupDb.Documents.Where(x => x.DocumentName == fileMetaInfo.File.ToLower()).FirstOrDefault();

                if (documentDb != null)
                {
                    if (documentDb.Md5 != fileMetaInfo.Md5)
                    {
                        // Upd DocumentGroup
                        documentGroupDb.Operation = (int)Operation.Upd;
                        documentGroupDb.DocumentGroupDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

                        // Upd Document
                        documentDb.Md5 = fileMetaInfo.Md5;
                        documentDb.Operation = (int)Operation.Upd;
                        isProcess = true;
                    }
                }
                else
                {
                    // Add new Document
                    documentGroupDb.Operation = (int)Operation.Upd;
                    documentGroupDb.DocumentGroupDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                    fileMetaInfo.Operation = Operation.Add;
                    fileMetaInfo.Identifier = Guid.NewGuid().ToString();
                    Document document = this.ReturnNewDocument(fileMetaInfo);

                    documentGroupDb.Documents.Add(document);
                    isProcess = true;
                }
            }

            return isProcess;
        }

        /// <summary>
        /// Create new Document
        /// </summary>
        /// <param name="fileMetaInfo"></param>
        /// <returns></returns>
        private Document ReturnNewDocument(XmlDocumentMetaInfo fileMetaInfo)
        {
            Document document = new Document();
            document.DocumentFormat = fileMetaInfo.Format;
            document.DocumentName = fileMetaInfo.File.ToLower();
            document.Identifier = fileMetaInfo.Identifier;
            document.Md5 = fileMetaInfo.Md5;
            document.Operation = (int)fileMetaInfo.Operation;
            document.Url = fileMetaInfo.Url;

            return document;
        }

        /// <summary>
        /// Тake value the directory path. 
        /// Gets all the files in filesMetaInfo and compress them into a zip. 
        /// </summary>
        /// <param name="docDir">Directory path</param>
        /// <param name="filesMetaInfo"></param>
        private void MemoryZip(string docDir, List<XmlDocumentMetaInfo> filesMetaInfo)
        {
            string zipFileName = docDir + ".zip";
            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFileName)))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression
                byte[] buffer = new byte[4096];
                foreach (var doc in filesMetaInfo)
                {
                    ZipEntry entry = new
                           ZipEntry(Path.GetFileName(doc.File));

                    // entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);
                    using (MemoryStream ms = new MemoryStream(doc.DataContent, 0, doc.DataContent.Length))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = ms.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        }
                        while (sourceBytes > 0);
                    }
                }

                s.Finish();
            }
        }
    }
}
