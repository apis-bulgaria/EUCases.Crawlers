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
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class Bverwg : BaseCrawler
    {
        public override void Start()
        {
            string url = "http://www.bverwg.de/entscheidungen/archiv.php";

            HtmlDocument doc = this.GetHtmlDocumentFromUrl(url);

            // Access the table with archives, gets all links from it and insert them into list
            HtmlNode tableBody = doc.DocumentNode.SelectSingleNode("//table[@class='archiv_nav']");

            List<string> urls = new List<string>();
            foreach (HtmlNode tr in tableBody.ChildNodes)
            {
                foreach (HtmlNode td in tr.ChildNodes)
                {
                    if (td.FirstChild != null)
                    {
                        HtmlNode a = td.FirstChild;

                        if (a.Attributes["href"] != null)
                        {
                            urls.Add(a.Attributes["href"].Value);
                        }
                    }
                }
            }

            foreach (var urlItem in urls)
            {
                this.OpenAllDocumentsForMonth("http://www.bverwg.de/" + urlItem);
            }
        }

        private void OpenAllDocumentsForMonth(string url)
        {
            HtmlDocument doc = this.GetHtmlDocumentFromUrl(url);
            HtmlNodeCollection allLinksMainNodes = doc.DocumentNode.SelectNodes("//div[@class='artikel ent_teaser']");

            foreach (HtmlNode linkDivNode in allLinksMainNodes)
            {
                HtmlNode nodeWithLink = linkDivNode.ChildNodes[3];
                var firstChild = nodeWithLink.ChildNodes[1];
                var nextChild = firstChild.ChildNodes[1];
                string urlItem = nextChild.Attributes["href"].Value;
                this.DownloadFullHtml("http://www.bverwg.de" + urlItem, linkDivNode.OuterHtml);
            }
        }

        private void DownloadFullHtml(string url, string divMetaData)
        {
            try
            {
                HtmlDocument doc = this.GetHtmlDocumentFromUrl(url);

                var articleNode = doc.DocumentNode.SelectSingleNode("//div[@class='artikel']");
                if (articleNode != null)
                {
                    string content = articleNode.OuterHtml;
                    byte[] dataContent = Encoding.UTF8.GetBytes(content);

                    url = url.Replace("amp;", string.Empty);
                    var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url));
                    string fileName = this.GetFileName(md5Name);

                    List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = fileName + ".html",
                        Url = url,
                        Format = "text/html",
                        Operation = Operation.None,
                        DataContent = dataContent
                    });

                    byte[] divDataContent = Encoding.UTF8.GetBytes(divMetaData);
                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = fileName + "_divmeta.html",
                        Url = url,
                        Format = "text/html",
                        Operation = Operation.None,
                        DataContent = divDataContent
                    });

                    if (documentMetaInfo.Count > 0)
                    {
                        this.DoOnDocDownloaded(documentMetaInfo, Path.GetFileNameWithoutExtension(fileName).ToLower().Trim());
                    }
                }
                else
                {
                    throw new ArgumentNullException("Artikel node not found.");
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo(url);
                CrawlerLog.LogException(ex);
            }
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
    }
}
