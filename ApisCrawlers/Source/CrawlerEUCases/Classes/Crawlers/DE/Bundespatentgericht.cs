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
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class Bundespatentgericht : BaseCrawler
    {
        public override void Start()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = 2000; year <= currentYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    HtmlDocument doc = this.GetHtmlDocumentFromUrl("http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/list.py?Gericht=bpatg&Art=en&Datum=" + year + "-" + month);
                    if (!(doc.DocumentNode.InnerText.IndexOf("Leider befinden sich keine Dokumente  aus dem") >= 0))
                    {
                        // Get all links for the first page
                        HtmlNodeCollection allTableNodesForMonth = this.GoToPageAndGetTableNodes("http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/list.py?Gericht=bpatg&Art=en&Datum=" + year + "-" + month);

                        // Get links for the other pages
                        HtmlNodeCollection innerTdsWithLastPageLink = doc.DocumentNode.SelectNodes("//td[@class='pagenumber']");
                        if (innerTdsWithLastPageLink != null)
                        {
                            string lastPageLink = innerTdsWithLastPageLink[innerTdsWithLastPageLink.Count - 1].ChildNodes[1].Attributes["href"].Value;
                            int numberOfLastPage = int.Parse(lastPageLink.Substring(lastPageLink.IndexOf("Seite=") + 6));

                            List<string> otherPagesLinks = new List<string>();
                            for (int i = 1; i <= numberOfLastPage; i++)
                            {
                                otherPagesLinks.Add("list.py?Gericht=bpatg&Art=en&Datum=" + year + "-" + month + "&Seite=" + i);
                            }

                            if (otherPagesLinks != null)
                            {
                                foreach (string pageLink in otherPagesLinks)
                                {
                                    HtmlNodeCollection tableNodes = this.GoToPageAndGetTableNodes("http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/" + pageLink);
                                    foreach (var node in tableNodes)
                                    {
                                        allTableNodesForMonth.Add(node);
                                    }
                                }
                            }
                        }

                        if (allTableNodesForMonth != null)
                        {
                            // Download html files
                            Parallel.ForEach(
                                allTableNodesForMonth,
                                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                                tableNode =>
                                {
                                    this.DownloadHtml(tableNode, "http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/");
                                });
                        }
                        else
                        {
                            CrawlerLog.LogError("Error in: http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/list.py?Gericht=bfh&Art=en&Datum=" + year + "-" + month);
                        }
                    }
                    else
                    {
                        CrawlerLog.LogError("Error in: http://juris.bundespatentgericht.de/cgi-bin/rechtsprechung/list.py?Gericht=bfh&Art=en&Datum=" + year + "-" + month);
                    }
                }
            }
        }

        private HtmlNodeCollection GoToPageAndGetTableNodes(string pageUrl)
        {
            HtmlDocument doc = this.GetHtmlDocumentFromUrl(pageUrl);

            HtmlNodeCollection tableNodes = doc.DocumentNode.SelectNodes("//tr[@class='roweven']|//tr[@class='rowodd']");

            return tableNodes;
        }

        private HtmlDocument GetHtmlDocumentFromUrl(string url)
        {
            // Create request to the page
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url.Replace("amp;", string.Empty));

            // Get response and insert response html in "htmlText" valiable
            request.UserAgent = "Bverwg Web Crawler";
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();

            // Load html text into html document
            HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlText);

            return doc;
        }

        private void DownloadHtml(HtmlNode tableNode, string baseUrl)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(tableNode.OuterHtml);
                var rootLinkNode = doc.DocumentNode.SelectSingleNode("//a[@class='besuchte']");

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
                this.AddDocument(rootLinkNode, baseUrl, documentMetaInfo);

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

            WebClient webCliewnt = new WebClient();
            byte[] documenContent = webCliewnt.DownloadData(documentUrl);
            documentName = this.ClearFileName(documentName);

            documentMetaInfo.Add(new XmlDocumentMetaInfo()
            {
                File = documentName,
                Url = documentUrl,
                Format = linkNode.Attributes["type"].Value,
                Operation = Operation.None,
                DataContent = documenContent
            });
        }
    }
}
