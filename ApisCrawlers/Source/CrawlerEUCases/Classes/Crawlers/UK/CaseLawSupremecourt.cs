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

    public class CaseLawSupremecourt : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        public void DoWork()
        {
            this.DownloadPdf();
        }

        public void DownloadPdf()
        {
            var url = "http://www.supremecourt.uk/decided-cases/";
            try
            {
                using (var client = new WebClient())
                {
                    var html = client.DownloadString(url);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var tdnodes = doc.DocumentNode.SelectNodes("//tr[@class='even']|//tr[@class='odd']");

                    Parallel.ForEach(
                    tdnodes,
                    new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                    tdnode =>
                    {
                        string link = string.Empty;

                        try
                        {
                            if (tdnode != null)
                            {
                                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                                var caseIdNode = tdnode.SelectSingleNode("td[@class='thirdColumn']");
                                string metaInfoName = "_metainfo.html";
                                string directoryName = string.Empty;
                                if (caseIdNode != null)
                                {
                                    directoryName = caseIdNode.InnerText.Replace('/', '_');
                                    directoryName = this.ClearFileName(directoryName);
                                    metaInfoName = directoryName + metaInfoName;
                                    metaInfoName = this.ClearFileName(metaInfoName);
                                }

                                var metaContent = Encoding.UTF8.GetBytes(tdnode.OuterHtml);
                                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                {
                                    File = metaInfoName,
                                    Url = url,
                                    Format = base.GetDataFormat(metaInfoName),
                                    Operation = Operation.None,
                                    DataContent = metaContent
                                });

                                var links = tdnode.SelectSingleNode("td[@class='fifthColumn']");
                                foreach (var anode in links.SelectNodes("a"))
                                {
                                    if (anode != null)
                                    {
                                        link = anode.GetAttributeValue("href", string.Empty);
                                        if (link.Contains(".pdf"))
                                        {
                                            using (var clientDownload = new WebClient())
                                            {
                                                if (!link.ToLower().Contains("http://"))
                                                {
                                                    link = url + link;
                                                    link = link.Trim();
                                                }

                                                byte[] dataContent = clientDownload.DownloadData(link);
                                                var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(link));
                                                string fileName = this.ClearFileName(md5Name);

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
                                }

                                if (directoryName != string.Empty)
                                {
                                    this.DoOnDocDownloaded(documentMetaInfo, directoryName);
                                }
                                else
                                {
                                    throw new ArgumentNullException("No directory name on tdNode: " + tdnode);
                                }
                            }
                        }
                        catch (WebException ex)
                        {
                            CrawlerLog.LogException(ex, link);
                        }
                    });
                }
            }
            catch (WebException ex)
            {
                CrawlerLog.LogException(ex, url);
            }
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
    }
}
