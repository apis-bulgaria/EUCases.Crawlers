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

namespace Crawler.Classes.Crawlers.DE
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class Bundesverfassungsgericht : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            string baseUrl = "http://www.bundesverfassungsgericht.de";
            string navigateUrl = "http://www.bundesverfassungsgericht.de/entscheidungen.html";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(navigateUrl);

            var linkNodes = doc.DocumentNode.SelectNodes("//div[@id='jahreskalender']//a");
            foreach (var linkNode in linkNodes)
            {
                string url = baseUrl + linkNode.Attributes["href"].Value;
                HtmlWeb webLinks = new HtmlWeb();
                HtmlDocument docLinks = webLinks.Load(url);
                var tableNodes = docLinks.DocumentNode.SelectNodes("//div[@class='entscheidung']");
                Parallel.ForEach(
                               tableNodes,
                               new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                               tableNode =>
                               {
                                   this.DownloadHtml(tableNode, baseUrl);
                               });
            }
        }

        private void DownloadHtml(HtmlNode tableNode, string baseUrl)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(tableNode.OuterHtml);
                var rootLinkNode = doc.DocumentNode.SelectSingleNode("//div[@class='aktenzeichen']//a");

                if (rootLinkNode == null)
                {
                    throw new ArgumentNullException("Null root link: " + tableNode.OuterHtml);
                }

                if (rootLinkNode.Attributes["href"] == null)
                {
                    throw new ArgumentNullException("Root link with null href: " + tableNode.OuterHtml);
                }

                // Info Document
                string documentUrl = baseUrl + rootLinkNode.Attributes["href"].Value.Replace("amp;", string.Empty);
                var infoContent = Encoding.UTF8.GetBytes(tableNode.OuterHtml);
                var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(documentUrl));
                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                string fileName = this.ClearFileName(md5Name);
                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = fileName + ".html",
                    Url = baseUrl,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });

                // Root Document
                this.AddDocument(rootLinkNode, baseUrl, documentMetaInfo);

                // Аdditional documents
                var additionalNodes = doc.DocumentNode.SelectNodes("//div[@class='kurztext']//a");

                if (additionalNodes != null)
                {
                    foreach (var additionalNode in additionalNodes)
                    {
                        this.AddDocument(additionalNode, baseUrl, documentMetaInfo);
                    }
                }

                if (documentMetaInfo.Count > 0)
                {
                    this.DoOnDocDownloaded(documentMetaInfo, fileName);
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo(tableNode.OuterHtml);
                CrawlerLog.LogException(ex);
            }
        }

        private void AddDocument(HtmlNode linkNode, string baseUrl, List<XmlDocumentMetaInfo> documentMetaInfo)
        {
            string documentName = linkNode.InnerText.Replace("&nbsp;", " ").Replace("/", "_");
            string documentUrl = baseUrl + linkNode.Attributes["href"].Value.Replace("amp;", string.Empty);
            documentName = this.ClearFileName(documentName);

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(documentUrl);
            var mainContent = doc.DocumentNode.SelectSingleNode("//div[@id='text']");
            byte[] documenContent = Encoding.UTF8.GetBytes(mainContent.OuterHtml);
            documentMetaInfo.Add(new XmlDocumentMetaInfo()
            {
                File = documentName,
                Url = documentUrl,
                Format = "text/htm",
                Operation = Operation.None,
                DataContent = documenContent
            });
        }
    }
}
