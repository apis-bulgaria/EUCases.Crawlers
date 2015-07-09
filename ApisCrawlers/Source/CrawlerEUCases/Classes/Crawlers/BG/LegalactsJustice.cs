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
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class LegalactsJustice : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            List<int> indexes = new List<int>(6272493);

            for (int i = 0; i < 6272493; i++)
            {
                indexes.Add(i);
            }

            string baseUrl = "http://legalacts.justice.bg/";

            Parallel.ForEach(
                indexes,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                index =>
                {
                    string url = "http://legalacts.justice.bg/ActDetail.aspx?actId=" + index;
                    this.ProcessSignlePage(url, baseUrl, index);
                });
        }

        private void ProcessSignlePage(string url, string baseUrl, int index)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                var contentDiv = doc.DocumentNode.SelectSingleNode("//div[@id='right-col']");

                if (contentDiv != null)
                {
                    // Info Document
                    var infoContent = Encoding.UTF8.GetBytes(contentDiv.OuterHtml);
                    string fileName = index + "_info";
                    List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = fileName,
                        Url = url,
                        Format = "text/htm",
                        Operation = Operation.None,
                        DataContent = infoContent
                    });

                    // Root Document
                    var rootLinkNode = doc.DocumentNode.SelectSingleNode("//a[@id='ctl00_ContentPlaceHolder1_hlinkActText']");

                    if (rootLinkNode == null)
                    {
                        throw new ArgumentNullException("Null root link: " + url);
                    }

                    string hrefLink = rootLinkNode.Attributes["href"].Value.Replace("amp;", string.Empty);
                    string documentUrl = baseUrl + hrefLink;
                    var linkAtributes = hrefLink.Split(new string[] { "Id=" }, StringSplitOptions.RemoveEmptyEntries);
                    string rootDocumentName = linkAtributes[1].Trim();

                    WebClient webCliewnt = new WebClient();
                    byte[] documenContent = webCliewnt.DownloadData(documentUrl);

                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = rootDocumentName,
                        Url = documentUrl,
                        Format = "text/htm",
                        Operation = Operation.None,
                        DataContent = documenContent
                    });

                    if (documentMetaInfo.Count > 0)
                    {
                        this.DoOnDocDownloaded(documentMetaInfo, index.ToString().ToLower().Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo(url);
                CrawlerLog.LogException(ex);
            }
        }
    }
}
