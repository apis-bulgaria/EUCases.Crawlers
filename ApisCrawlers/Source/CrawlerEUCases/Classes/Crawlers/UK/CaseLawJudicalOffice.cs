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

namespace Crawler.Classes.Crawlers.UK
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class CaseLawJudicalOffice : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        public void DoWork()
        {
            int endYear = DateTime.Now.Year;
            for (int year = 2009; year <= endYear; year++)
            {
                HashSet<Uri> urls = this.GetAllCaseLawLinks(year);
                this.DownloadCaseLaw(urls);
            }
        }

        public HashSet<Uri> GetAllCaseLawLinks(int year)
        {
            string url = "http://www.judiciary.gov.uk/media/judgments/" + year;
            HashSet<Uri> urls = new HashSet<Uri>();
            try
            {
                using (var client = new WebClient())
                {
                    var html = client.DownloadString(url);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var nodes = doc.DocumentNode.SelectNodes("//a[@class='mainLink']");

                    foreach (var node in nodes)
                    {
                        var link = node.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(link))
                        {
                            Uri uri = null;
                            if (link.Contains("http://www.judiciary.gov.uk"))
                            {
                                uri = new Uri(link);
                            }
                            else
                            {
                                uri = new Uri("http://www.judiciary.gov.uk" + link);
                            }

                            urls.Add(uri);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogException(ex, url.ToString());
            }

            return urls;
        }

        public void DownloadCaseLaw(HashSet<Uri> urls)
        {
            Parallel.ForEach(
                urls,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                url =>
                {
                    string link = string.Empty;

                    try
                    {
                        using (var client = new WebClient())
                        {
                            var html = client.DownloadString(url);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                            var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url.ToString()));
                            string folderName = this.ClearFileName(md5Name);
                            var divMainContent = doc.DocumentNode.SelectSingleNode("//div[@id='MainControl_mainColumnContainer']");
                            if (divMainContent != null)
                            {
                                var mainContent = Encoding.UTF8.GetBytes(divMainContent.OuterHtml);

                                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                {
                                    File = folderName + ".html",
                                    Url = link,
                                    Format = "text/html",
                                    Operation = Operation.None,
                                    DataContent = mainContent
                                });
                            }

                            var divNode = doc.DocumentNode.SelectSingleNode("//div[@class='linkList']");
                            if (divNode != null)
                            {
                                try
                                {
                                    foreach (var node in divNode.SelectNodes("ul//li//a"))
                                    {
                                        link = node.GetAttributeValue("href", string.Empty);
                                        if (!string.IsNullOrEmpty(link))
                                        {
                                            if (!link.ToLower().Contains("http"))
                                            {
                                                link = "http://www.judiciary.gov.uk/" + link;
                                            }

                                            byte[] dataContent = client.DownloadData(link);
                                            var md5FileName = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url.ToString()));
                                            string fileName = this.ClearFileName(md5FileName);

                                            documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                            {
                                                File = fileName + base.GetFileExtension(link),
                                                Url = link,
                                                Format = base.GetDataFormat(link),
                                                Operation = Operation.None,
                                                DataContent = dataContent
                                            });
                                        }
                                    }
                                }
                                catch (WebException ex)
                                {
                                    CrawlerLog.LogException(ex, link.ToString());
                                }

                                if (documentMetaInfo.Count > 0)
                                {
                                    this.DoOnDocDownloaded(documentMetaInfo, folderName);
                                }
                            }
                            else
                            {
                                throw new WebException("This link dos not contains linkList for download!");
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        CrawlerLog.LogException(ex, url.ToString());
                    }
                });
        }

        public override string GetFileName(string link)
        {
            link = link.ToLower();
            string fname = "error.pdf";

            if (link.Contains("pdf") || link.Contains("html"))
            {
                int lastSlash = link.LastIndexOf('/');

                if (lastSlash > 0)
                {
                    fname = link.Substring(lastSlash + 1);
                }
            }
            else
            {
                if (link.Contains("id="))
                {
                    int idIndex = link.LastIndexOf("id=");
                    fname = link.Substring(idIndex + 3);
                }
            }

            return fname;
        }

        private string GetDirectoryName(string link)
        {
            string dname = "Error";
            int lastSlash = link.LastIndexOf('/');

            if (lastSlash > 0)
            {
                dname = link.Substring(lastSlash + 1);
            }

            return dname;
        }
    }
}
