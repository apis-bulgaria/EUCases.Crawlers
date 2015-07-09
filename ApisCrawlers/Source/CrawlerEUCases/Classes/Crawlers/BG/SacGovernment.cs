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

namespace Crawler.Classes.Crawlers.BG
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class SacGovernment : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            int currentYear = DateTime.Now.Year;
            for (int year = 2010; year <= currentYear; year++)
            {
                for (int caseNumber = 1; caseNumber < 20000; caseNumber++)
                {
                    string searchUrl = "http://www.sac.government.bg/court22.nsf/($All)?SearchView&Query=FIELD%20c_No%3D2%20AND%20FIELD%20c_caseYear%3D2013%20AND%20FIELD%20form%3DCase&SearchWV=0&Seq=4&SearchFuzzy=0?SearchView&SearchWV=0&SearchFuzzy=0&Query=FIELD%20c_No=" + caseNumber + "%20AND%20FIELD%20c_caseYear=" + year + "%20AND%20FIELD%20form=Case";

                    try
                    {
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(searchUrl);

                        var links = doc.DocumentNode.SelectNodes("//a");
                        string documentRoutingLink = string.Empty;
                        foreach (var link in links)
                        {
                            if (link.HasAttributes)
                            {
                                var documentLink = link.Attributes["href"].Value;
                                if (documentLink.Contains("OpenDocument"))
                                {
                                    documentRoutingLink = documentLink;
                                }
                            }
                        }

                        if (documentRoutingLink != string.Empty)
                        {
                            string fileName = caseNumber + "_" + year;
                            this.DownloadDocument("http://www.sac.government.bg" + documentRoutingLink, fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        CrawlerLog.LogInfo("Error in: " + searchUrl);
                        CrawlerLog.LogException(ex);
                    }
                }
            }
        }

        private void DownloadDocument(string documentRoutingLink, string fileName)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                web.OverrideEncoding = Encoding.GetEncoding("KOI8-R");
                HtmlDocument doc = web.Load(documentRoutingLink);

                var content = doc.DocumentNode.SelectSingleNode("//div[@class='document-frame']");

                string contentUtf8 = this.ConvertAnsiToUtf8(content.OuterHtml);
                var infoContent = Encoding.UTF8.GetBytes(contentUtf8);
                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = fileName + ".html",
                    Url = documentRoutingLink,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });

                // Additional documents
                var additionalLinks = doc.DocumentNode.SelectNodes("//a");
                if (additionalLinks != null)
                {
                    foreach (var item in additionalLinks)
                    {
                        string link = item.Attributes["href"].Value;
                        string name = item.InnerText;
                        if (name.ToLower().Contains("решение") || name.ToLower().Contains("определение"))
                        {
                            name = name.ToLower().Replace("решение", "reshenie").Replace("определение", "opredelenie").Replace("&#8470;", string.Empty).Replace(" от ", "_");
                            this.AddAdditionalDocument(name, link, documentMetaInfo);
                        }
                    }
                }

                if (documentMetaInfo.Count > 0)
                {
                    this.DoOnDocDownloaded(documentMetaInfo, fileName.ToLower().Trim());
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo("Error in: " + documentRoutingLink);
                CrawlerLog.LogException(ex);
            }
        }

        private void AddAdditionalDocument(string fileName, string additioanlDocLink, List<XmlDocumentMetaInfo> documentMetaInfo)
        {
            additioanlDocLink = "http://www.sac.government.bg" + additioanlDocLink;
            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding("KOI8-R");
            HtmlDocument doc = web.Load(additioanlDocLink);

            var content = doc.DocumentNode.SelectSingleNode("//div[@class='document-frame']");
            if (content == null)
            {
                content = doc.DocumentNode.SelectSingleNode("//div[@id='body-content']");
            }

            string contentUtf8 = this.ConvertAnsiToUtf8(content.OuterHtml);
            var infoContent = Encoding.UTF8.GetBytes(contentUtf8);

            documentMetaInfo.Add(new XmlDocumentMetaInfo()
            {
                File = fileName + ".html",
                Url = additioanlDocLink,
                Format = "text/htm",
                Operation = Operation.None,
                DataContent = infoContent
            });
        }

        private string ConvertAnsiToUtf8(string cyrString)
        {
            byte[] cyrilycByteArray = Encoding.GetEncoding("KOI8-R").GetBytes(cyrString);
            byte[] strData = Encoding.Convert(Encoding.GetEncoding("KOI8-R"), Encoding.UTF8, cyrilycByteArray);

            return Encoding.UTF8.GetString(strData);
        }
    }
}
