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

namespace Crawler.Classes.Crawlers.AT
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

    public class RisBka : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        public void DoWork()
        {
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Vfgh&Entscheidungsart=Undefined&Sammlungsnummer=&Index=&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=15.04.2014&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Vwgh&Entscheidungsart=Undefined&Sammlungsnummer=&Index=&AenderungenSeit=Undefined&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=15.04.2014&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Justiz&Gericht=&Rechtssatznummer=&Rechtssatz=&Fundstelle=&AenderungenSeit=Undefined&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=15.04.2014&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Bvwg&Entscheidungsart=Undefined&SucheNachRechtssatz=True&SucheNachText=True&GZ=&VonDatum=01.01.2014&BisDatum=15.04.2014&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Lvwg&Entscheidungsart=Undefined&Bundesland=Undefined&SucheNachRechtssatz=True&SucheNachText=True&GZ=&VonDatum=01.01.2014&BisDatum=15.04.2014&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            //
            // https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Normenliste&Titel=&Norm=&Index=&Typ=&Kundmachungsorgan=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1
            string dateTimeNow = DateTime.Now.ToString("dd.MM.yyy");

            List<string> rootUrls = new List<string>() 
            {
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Vfgh&Entscheidungsart=Undefined&Sammlungsnummer=&Index=&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=" + dateTimeNow + "&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1",
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Vwgh&Entscheidungsart=Undefined&Sammlungsnummer=&Index=&AenderungenSeit=Undefined&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=" + dateTimeNow + "&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1",
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Justiz&Gericht=&Rechtssatznummer=&Rechtssatz=&Fundstelle=&AenderungenSeit=Undefined&SucheNachRechtssatz=True&SucheNachText=False&GZ=&VonDatum=&BisDatum=" + dateTimeNow + "&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1",
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Bvwg&Entscheidungsart=Undefined&SucheNachRechtssatz=True&SucheNachText=True&GZ=&VonDatum=01.01.2014&BisDatum=" + dateTimeNow + "&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1",
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Lvwg&Entscheidungsart=Undefined&Bundesland=Undefined&SucheNachRechtssatz=True&SucheNachText=True&GZ=&VonDatum=01.01.2014&BisDatum=" + dateTimeNow + "&Norm=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1",
                
                "https://www.ris.bka.gv.at/Ergebnis.wxe?Abfrage=Normenliste&Titel=&Norm=&Index=&Typ=&Kundmachungsorgan=&ImRisSeit=Undefined&ResultPageSize=100&Suchworte=&Position=1"
            };

            string baseUrl = "https://www.ris.bka.gv.at";

            foreach (var rootUrl in rootUrls)
            {
                string navigationUrl = rootUrl;
                bool isNext = true;
                do
                {
                    navigationUrl = navigationUrl.Replace("amp;", string.Empty);
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(navigationUrl);

                    var nextPage = doc.DocumentNode.SelectSingleNode("//div[@id='TopMainTasks']//span[@class='Pages']//span[@class='Next']//a");

                    if (nextPage != null && nextPage.Attributes["href"] != null)
                    {
                        navigationUrl = baseUrl + nextPage.Attributes["href"].Value;
                    }
                    else
                    {
                        isNext = false;
                    }

                    var contents = doc.DocumentNode.SelectNodes("//div[@id='Srmp_MainContent_DocumentsList']//tr[@class='bocListDataRow']");

                    Parallel.ForEach(
                                contents,
                                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                                content =>
                                {
                                    this.DownloadHtml(content, baseUrl);
                                });
                }
                while (isNext);
            }
        }

        private void DownloadHtml(HtmlNode tableNode, string baseUrl)
        {
            try
            {
                // Root Document
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(tableNode.OuterHtml);
                var rootLinkNode = doc.DocumentNode.SelectSingleNode("//a[@class='nonWrappingCell']|//a[@class='wrappingCell']");

                if (rootLinkNode == null)
                {
                    throw new ArgumentNullException("Null root link: " + tableNode.OuterHtml);
                }

                // Info Document
                var infoContent = Encoding.UTF8.GetBytes(tableNode.OuterHtml);
                string documentId = this.GetDocumentId(rootLinkNode.Attributes["href"].Value);

                if (documentId == string.Empty)
                {
                    documentId = MD5HashHelper.GetMd5Hash(infoContent);
                }

                string fileName = this.ClearFileName(documentId);
                List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                {
                    File = fileName + "_meta.html",
                    Url = baseUrl,
                    Format = "text/htm",
                    Operation = Operation.None,
                    DataContent = infoContent
                });

                this.AddDocument(rootLinkNode, baseUrl, documentMetaInfo, fileName);

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

        private void AddDocument(HtmlNode linkNode, string baseUrl, List<XmlDocumentMetaInfo> documentMetaInfo, string fileName)
        {
            string documentUrl = baseUrl + linkNode.Attributes["href"].Value.Replace("amp;", string.Empty);

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(documentUrl);
            var content = doc.DocumentNode.SelectSingleNode("//div[@id='main']");
            byte[] documenContent = Encoding.UTF8.GetBytes(content.OuterHtml);

            documentMetaInfo.Add(new XmlDocumentMetaInfo()
            {
                File = fileName + ".html",
                Url = documentUrl,
                Format = "text/htm",
                Operation = Operation.None,
                DataContent = documenContent
            });
        }

        private string GetDocumentId(string url)
        {
            url = url.Replace("amp;", string.Empty);
            string nummer = "Dokumentnummer=";
            var atributes = url.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var dokumentnummer = atributes.Where(x => x.Contains(nummer)).FirstOrDefault();
            if (!string.IsNullOrEmpty(dokumentnummer))
            {
                return dokumentnummer.Substring(nummer.Length, dokumentnummer.Length - nummer.Length);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
