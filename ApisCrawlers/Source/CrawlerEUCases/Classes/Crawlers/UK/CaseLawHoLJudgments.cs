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

    public class CaseLawHoLJudgments : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        public void DoWork()
        {
            HashSet<string> urls = this.GetAllCaseLawLinks();
            this.ProcessDownloadHtml(urls);
        }

        public HashSet<string> GetAllCaseLawLinks()
        {
            string url = "http://www.publications.parliament.uk/pa/ld/ldjudgmt.htm";
            HashSet<string> urls = new HashSet<string>();
            using (var client = new WebClient())
            {
                var html = client.DownloadString(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var nodes = doc.DocumentNode.SelectNodes("//a");

                foreach (var node in nodes)
                {
                    var link = node.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(link))
                    {
                        if (link.Contains("ldjudgmt"))
                        {
                            if (link.Contains("../"))
                            {
                                link = link.Replace("../", "/pa/");
                            }

                            int trimIndex = link.IndexOf('#');
                            if (trimIndex > 0)
                            {
                                link = link.Substring(0, trimIndex);
                            }

                            link = link.Trim();
                            urls.Add("http://www.publications.parliament.uk" + link);
                        }
                    }
                }
            }

            return urls;
        }

        public void ProcessDownloadHtml(HashSet<string> urls)
        {
            Parallel.ForEach(
                urls,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                url =>
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();
                            bool existNext = false;
                            var md5FolderName = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url));
                            string folderName = this.ClearFileName(md5FolderName);

                            string currentUrl = url;
                            do
                            {
                                existNext = false;
                                var html = client.DownloadString(currentUrl);
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(html);

                                // TODO: if no id? there is 1 link http://www.publications.parliament.uk/pa/ld199697/ldjudgmt/jd961121/smith01.htm
                                var divNode = doc.GetElementbyId("maincontent");

                                if (divNode == null)
                                {
                                    divNode = doc.GetElementbyId("maincontent1");
                                }

                                if (divNode != null)
                                {
                                    byte[] dataContent = System.Text.Encoding.UTF8.GetBytes(divNode.OuterHtml);
                                    var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(currentUrl));
                                    string fileName = this.ClearFileName(md5Name);

                                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                    {
                                        File = fileName + base.GetFileExtension(currentUrl),
                                        Url = url,
                                        Format = base.GetDataFormat(currentUrl),
                                        Operation = Operation.None,
                                        DataContent = dataContent
                                    });

                                    currentUrl = CheckForNextPage(divNode, currentUrl);
                                    if (currentUrl != string.Empty)
                                    {
                                        existNext = true;
                                    }
                                }
                            }
                            while (existNext);

                            if (documentMetaInfo.Count > 0)
                            {
                                this.DoOnDocDownloaded(documentMetaInfo, folderName);
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        CrawlerLog.LogException(ex, url);
                    }
                });
        }

        private string CheckForNextPage(HtmlNode divNode, string url)
        {
            string newUrl = string.Empty;

            var anodes = divNode.SelectNodes("//a");
            if (anodes != null)
            {
                foreach (var anode in anodes)
                {
                    if (anode.InnerText == "Continue")
                    {
                        newUrl = anode.GetAttributeValue("href", string.Empty);
                    }
                }
            }

            if (newUrl != string.Empty)
            {
                int lastSlash = url.LastIndexOf('/');
                if (lastSlash > 0 && lastSlash < url.Length - 1)
                {
                    newUrl = url.Substring(0, lastSlash + 1) + newUrl;
                }
            }

            return newUrl;
        }
    }
}
