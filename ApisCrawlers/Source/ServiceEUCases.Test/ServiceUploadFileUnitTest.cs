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

namespace ServiceEUCases.Test
{
    using System;
    using System.Fakes;
    using MetainfoEUCases;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Data;
    using Repositories;
    using Repositories.Fakes;
    using Classes;

    [TestClass]
    public class ServiceUploadFileUnitTest
    {
        [TestMethod]
        public void NormalGroupAddTest()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                CrawlerLog log = new CrawlerLog();
                DocumentGroup documentGroup = new DocumentGroup();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    GetOrCreateCrawlerIdString = (crawlerName) => { return 1; },
                    AddDocumentGroupDocumentGroup = (newDocumentGroup) => { documentGroup = newDocumentGroup; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);

                // ReturnMessage
                Assert.AreEqual("Ok", msgActual, "Service return message.");

                // AddDocumentGroup
                Assert.AreEqual(xmlDocumentGroup.Identifier, documentGroup.Identifier, "Identifier");
                Assert.AreEqual(1, documentGroup.CrawlerId, "CrawlerId");
                Assert.AreEqual(0, documentGroup.Operation, "Operation");
                Assert.AreEqual(xmlDocumentGroup.Document.Count, documentGroup.Documents.Count, "DocumentsCount");

                // LogAsserts
                Assert.AreEqual(xmlDocumentGroup.Identifier, log.Identifier, "IdentifierLog");
                Assert.AreEqual(true, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
                Assert.AreEqual(xmlDocumentGroup.Crawler, log.CrawlerName, "CrawlerNameLog");
                Assert.AreEqual(0, log.Operation, "OperationLog");
            }
        }

        [TestMethod]
        public void NormalGroupUpdateTest()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            xmlDocumentGroup.Operation = Operation.Upd;
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                string ident = string.Empty;
                CrawlerLog log = new CrawlerLog();
                DocumentGroup documentGroup = new DocumentGroup();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return xmlDocumentGroup.Identifier; },
                    GetOrCreateCrawlerIdString = (crawlerName) => { return 1; },
                    AddDocumentGroupDocumentGroup = (newDocumentGroup) => { documentGroup = newDocumentGroup; },
                    DeleteDocumentGroupString = (identifier) => { ident = identifier; return 1; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);

                // ReturnMessage
                Assert.AreEqual("Ok", msgActual, "Service return message.");

                // DeleteDocumentGroup
                Assert.AreEqual(xmlDocumentGroup.Identifier, ident, "Identifier");

                // AddDocumentGroup
                Assert.AreEqual(xmlDocumentGroup.Identifier, documentGroup.Identifier, "Identifier");
                Assert.AreEqual(1, documentGroup.CrawlerId, "CrawlerId");
                Assert.AreEqual(1, documentGroup.Operation, "Operation");
                Assert.AreEqual(xmlDocumentGroup.Document.Count, documentGroup.Documents.Count, "DocumentsCount");

                // LogAsserts
                Assert.AreEqual(xmlDocumentGroup.Identifier, log.Identifier, "IdentifierLog");
                Assert.AreEqual(true, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
                Assert.AreEqual(xmlDocumentGroup.Crawler, log.CrawlerName, "CrawlerNameLog");
                Assert.AreEqual(1, log.Operation, "OperationLog");
            }
        }

        #region ValidationTests
        [TestMethod]
        public void CrawlerNameXmlValidationTestNull()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            xmlDocumentGroup.Crawler = null;
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                CrawlerLog log = new CrawlerLog();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);
                string msgExpected = "Error: Validation Exception - The required attribute 'crawler' is missing.\r\n";

                Assert.AreEqual(msgExpected, msgActual, "Service return message.");
                Assert.AreEqual(false, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
            }
        }

        [TestMethod]
        public void IdentifierXmlValidationTestInvalid()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            xmlDocumentGroup.Identifier = "YYYPPPOOORRR";
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                CrawlerLog log = new CrawlerLog();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);
                string msgExpected = "Error: Validation Exception - The 'identifier' attribute has an invalid value according to its data type.\r\n";

                Assert.AreEqual(msgExpected, msgActual, "Service return message.");
                Assert.AreEqual(false, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
            }
        }

        [TestMethod]
        public void FormatXmlValidationTestInvalid()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            xmlDocumentGroup.Format = "application/badapplication";
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                CrawlerLog log = new CrawlerLog();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);
                string msgExpected = "Error: Validation Exception - The 'format' attribute has an invalid value according to its data type.\r\n";

                Assert.AreEqual(msgExpected, msgActual, "Service return message.");
                Assert.AreEqual(false, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
            }
        }

        [TestMethod]
        public void ZipXmlValidationTestNull()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");
            uploadDocumentGroup.Data = null;

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                CrawlerLog log = new CrawlerLog();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);
                string msgExpected = "Error: Validation Exception - ZipData length is zero or ZipData is NULL, document idenfier: " + xmlDocumentGroup.Identifier + " and Operation: Add";

                Assert.AreEqual(msgExpected, msgActual, "Service return message.");
                Assert.AreEqual(false, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
            }
        }

        [TestMethod]
        public void IdentifierGroupDeleteTestNull()
        {
            XmlDocumentGroup xmlDocumentGroup = XmlHelper.GetXmlDocumentGroup(@".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.xml");
            xmlDocumentGroup.Operation = Operation.Del;
            UploadDocumentGroup uploadDocumentGroup = XmlHelper.GetUploadDocumentGroup(xmlDocumentGroup, @".\DataContent\NormalContent\cc54a49bf188f994899cb1c954bb795f.zip");
            uploadDocumentGroup.Data = null;

            using (ShimsContext.Create())
            {
                ShimDateTime.NowGet = () => new DateTime(2000, 1, 1);

                string ident = string.Empty;
                CrawlerLog log = new CrawlerLog();
                DocumentGroup documentGroup = new DocumentGroup();
                IRepository fakeRepository = new StubIRepository
                {
                    GetIdentifierString = (identifier) => { return string.Empty; },
                    GetOrCreateCrawlerIdString = (crawlerName) => { return 1; },
                    AddDocumentGroupDocumentGroup = (newDocumentGroup) => { documentGroup = newDocumentGroup; },
                    DeleteDocumentGroupString = (identifier) => { ident = identifier; return 1; },
                    AddNewLogCrawlerLog = (crawlerLog) => { log = crawlerLog; }
                };

                WcfHost eu = new WcfHost(fakeRepository);
                var msgActual = eu.UploadFile(uploadDocumentGroup);
                string msgExpected = "Error: Validation Exception - No document idenfier: " + xmlDocumentGroup.Identifier + " .You can't use Operation: Del";

                // ReturnMessage
                Assert.AreEqual(msgExpected, msgActual, "Service return message.");

                // LogAsserts
                Assert.AreEqual(xmlDocumentGroup.Identifier, log.Identifier, "IdentifierLog");
                Assert.AreEqual(false, log.IsSuccess, "IsSuccess");
                Assert.AreEqual(new DateTime(2000, 1, 1), log.LogDate, "LogDate");
                Assert.AreEqual(xmlDocumentGroup.Crawler, log.CrawlerName, "CrawlerNameLog");
                Assert.AreEqual(2, log.Operation, "OperationLog");
            }
        }
        #endregion
    }
}
