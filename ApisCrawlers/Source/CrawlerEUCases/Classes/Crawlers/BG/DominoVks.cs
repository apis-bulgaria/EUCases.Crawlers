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
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class DominoVks : BaseCrawler
    {
        public override void Start()
        {
            List<int> indexes = new List<int>(100000);

            for (int i = 2500; i < 300000; i++)
            {
                indexes.Add(i);
            }

            int currentYear = DateTime.Now.Year;
            for (int year = 2010; year <= currentYear; year++)
            {
                Parallel.ForEach(
                    indexes,
                    new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                    index =>
                    {
                        SearchForDocument(year, index);
                    });
            }
        }

        public void SearchForDocument(int year, int incomingNumber)
        {
            string url = @"http://domino.vks.bg/bcap%5Cscc%5CWebData.nsf/Cases?SearchView&query=FIELD%20un0_IskVhNr%20=" + incomingNumber + "%20AND%20FIELD%20ud0_dataPodavane%20%3E=%201.1." + year + "%20AND%20FIELD%20ud0_DataPodavane%20%3C=%2031.12." + year;
            string fileName = incomingNumber + "_" + year;
            try
            {
                var web = new HtmlWeb();
                web.OverrideEncoding = Encoding.GetEncoding(1251);
                HtmlAgilityPack.HtmlDocument htmlDoc = web.Load(url);
                var form = htmlDoc.DocumentNode.SelectNodes("//form[@name='_DominoForm']").First();
                var childTbales = form.SelectNodes("//table");

                if (childTbales.Count >= 6)
                {
                    var table = childTbales[5];
                    var tr = table.SelectNodes("tr")[1];
                    var link = "http://domino.vks.bg" + tr.SelectNodes("td")[1].FirstChild.Attributes["href"].Value;

                    this.OpenMainDocumentPage(link, fileName);
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo("Error in: " + url);
                CrawlerLog.LogException(ex);
            }
        }

        public void OpenMainDocumentPage(string url, string fileName)
        {
            try
            {
                var web = new HtmlWeb();
                web.OverrideEncoding = Encoding.GetEncoding(1251);
                HtmlAgilityPack.HtmlDocument htmlDoc = web.Load(url);
                var form = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerHtml;

                // Info Document
                var infoContent = Encoding.UTF8.GetBytes(this.ConvertCyrilicToUtf8(form));
                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = fileName,
                    Url = url,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });

                // Additional documents
                var additionalLinks = htmlDoc.DocumentNode.SelectNodes("//a");
                if (additionalLinks != null)
                {
                    foreach (var item in additionalLinks)
                    {
                        this.AddAdditionalDocument(item.Attributes["href"].Value, documentMetaInfo);
                    }
                }

                if (documentMetaInfo.Count > 0)
                {
                    this.DoOnDocDownloaded(documentMetaInfo, fileName.ToLower().Trim());
                }
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo("Error in: " + url);
                CrawlerLog.LogException(ex);
            }
        }

        private void AddAdditionalDocument(string additioanlDocLink, List<XmlDocumentMetaInfo> documentMetaInfo)
        {
            try
            {
                var web = new HtmlWeb();
                additioanlDocLink = "http://domino.vks.bg" + additioanlDocLink;
                web.OverrideEncoding = Encoding.GetEncoding(1251);
                HtmlAgilityPack.HtmlDocument htmlDoc = web.Load(additioanlDocLink);
                var form = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerHtml;

                // Info Document
                var infoContent = Encoding.UTF8.GetBytes(this.ConvertCyrilicToUtf8(form));
                int index = additioanlDocLink.LastIndexOf('/') + 1;
                var md5Name = additioanlDocLink.Substring(index, additioanlDocLink.Length - index);

                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = md5Name,
                    Url = additioanlDocLink,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });
            }
            catch (Exception ex)
            {
                CrawlerLog.LogInfo("Error in: " + additioanlDocLink);
                CrawlerLog.LogException(ex);
            }
        }

        private string ConvertCyrilicToUtf8(string cyrString)
        {
            byte[] cyrilycByteArray = Encoding.GetEncoding(1251).GetBytes(cyrString);
            byte[] strData = Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8, cyrilycByteArray);

            return Encoding.UTF8.GetString(strData);
        }
    }
}
