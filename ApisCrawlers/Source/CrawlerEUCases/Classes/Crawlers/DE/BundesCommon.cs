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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public abstract class BundesCommon : BaseCrawler
    {
        protected void Download(string startUrl, string searchUrl, int startYear = 0)
        {
            if (startYear == 0)
            {
                startYear = DateTime.Now.Year - 4;
            }

            int currentYear = DateTime.Now.Year;
            for (int year = startYear; year <= currentYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(startUrl + searchUrl + year + "-" + month);
                    if (!(doc.DocumentNode.InnerText.IndexOf("Leider befinden sich keine Dokumente  aus dem") >= 0))
                    {
                        HtmlNode parentnode = new HtmlNode(HtmlNodeType.Document, new HtmlDocument(), 0);
                        HtmlNodeCollection allTableNodesForMonth = new HtmlNodeCollection(parentnode);

                        this.GoToPageAndGetTableNodes(startUrl, searchUrl + year + "-" + month, allTableNodesForMonth);

                        if (allTableNodesForMonth != null)
                        {
                            // Download html files
                            Parallel.ForEach(
                                allTableNodesForMonth,
                                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                                tableNode =>
                                {
                                    this.DownloadHtml(tableNode, startUrl + "/cgi-bin/rechtsprechung/");
                                });
                        }
                        else
                        {
                            CrawlerLog.LogError("Error in: " + startUrl + searchUrl + year + "-" + month);
                        }
                    }
                    else
                    {
                        CrawlerLog.LogError("Error in: " + startUrl + searchUrl + year + "-" + month);
                    }
                }
            }
        }

        private void GoToPageAndGetTableNodes(string startUrl, string pageUrl, HtmlNodeCollection allTableNodesForMonth)
        {
            string url = startUrl + pageUrl;
            bool haveNextPage = true;
            do
            {
                // Get all links for the first page
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                HtmlNodeCollection tableNodes = doc.DocumentNode.SelectNodes("//tr[@class='roweven']|//tr[@class='rowodd']");
                foreach (var node in tableNodes)
                {
                    allTableNodesForMonth.Add(node);
                }

                // Get links for the other pages
                HtmlNode nextPage = null;
                if (doc.DocumentNode.SelectNodes("//td[@class='pagenumber']") != null)
                {
                    nextPage = doc.DocumentNode.SelectNodes("//td[@class='pagenumber']").LastOrDefault();
                }

                if (nextPage != null)
                {
                    var linkNode = nextPage.FirstChild;
                    if (linkNode.Attributes["href"] != null)
                    {
                        url = startUrl + "/cgi-bin/rechtsprechung/" + linkNode.Attributes["href"].Value;
                        url = url.Replace("amp;", string.Empty);
                    }
                    else
                    {
                        haveNextPage = false;
                    }
                }
                else
                {
                    haveNextPage = false;
                }
            }
            while (haveNextPage);
        }

        private void DownloadHtml(HtmlNode tableNode, string baseUrl)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(tableNode.OuterHtml);
                var rootLinkNode = doc.DocumentNode.SelectSingleNode("//a[@class='doklink']");

                if (rootLinkNode == null)
                {
                    throw new ArgumentNullException("Null root link: " + tableNode.OuterHtml);
                }

                if (rootLinkNode.Attributes["href"] == null)
                {
                    throw new ArgumentNullException("Root link with null href: " + tableNode.OuterHtml);
                }

                string documentUrl = baseUrl + rootLinkNode.Attributes["href"].Value.Replace("amp;", string.Empty);

                // Info Document
                var infoContent = Encoding.UTF8.GetBytes(tableNode.OuterHtml);

                var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(documentUrl));
                string fileName = this.ClearFileName(md5Name);

                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = fileName + ".html",
                    Url = baseUrl,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });

                // Root Document
                this.AddDocument(rootLinkNode, baseUrl, documentMetaInfo, true);

                // Аdditional documents
                var additionalNodes = doc.DocumentNode.SelectNodes("//a[@class='linklink']");

                if (additionalNodes != null)
                {
                    foreach (var additionalNode in additionalNodes)
                    {
                        this.AddDocument(additionalNode, baseUrl, documentMetaInfo, false);
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

        private void AddDocument(HtmlNode linkNode, string baseUrl, List<XmlDocumentMetaInfo> documentMetaInfo, bool isRoot)
        {
            string documentName = linkNode.InnerText.Replace("&nbsp;", " ").Replace("/", "_");
            string documentUrl = baseUrl + linkNode.Attributes["href"].Value.Replace("amp;", string.Empty);

            if (baseUrl == "http://juris.bundesgerichtshof.de/cgi-bin/rechtsprechung/" && isRoot)
            {
                documentUrl += "&Blank=1.pdf";
                documentName += ".pdf";
            }
            else
            {
                documentName += ".html";
            }

            documentName = this.ClearFileName(documentName);
            byte[] documenContent = this.ReadFully(documentUrl);

            documentMetaInfo.Add(new XmlDocumentMetaInfo()
            {
                File = documentName,
                Url = documentUrl,
                Format = this.GetDataFormat(documentName),
                Operation = Operation.None,
                DataContent = documenContent
            });
        }

        private byte[] ReadFully(string documentUrl)
        {
            var request = HttpWebRequest.Create(documentUrl);
            (request as HttpWebRequest).KeepAlive = false;
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}
