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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using HtmlAgilityPack;

    public class LegislationGesetze : LegislationCommon
    {
        public override void Start()
        {
            this.DoWork();
        }

        protected override List<string> GetLinks()
        {
            string url = "http://www.gesetze-im-internet.de/aktuell.html";
            string baseUrl = "http://www.gesetze-im-internet.de";
            List<string> alphabetUrls = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//a[@class='alphabet']");

            foreach (var rootNavigationLink in linkNodes)
            {
                if (rootNavigationLink.Attributes["href"] != null)
                {
                    var href = rootNavigationLink.Attributes["href"].Value;
                    alphabetUrls.Add(baseUrl + href.TrimStart('.'));
                }
                else
                {
                    CrawlerLog.LogInfo("Link node without atribute 'href' :" + rootNavigationLink.InnerText);
                }
            }

            List<string> xmlUrls = new List<string>();
            foreach (var alphabetUrl in alphabetUrls)
            {
                HtmlWeb alphabetWeb = new HtmlWeb();
                HtmlDocument alphabetDoc = alphabetWeb.Load(alphabetUrl);
                var xmlLinkNodes = alphabetDoc.DocumentNode.SelectNodes("//a");

                foreach (var xmlLinkNode in xmlLinkNodes)
                {
                    if (xmlLinkNode.Attributes["href"] != null)
                    {
                        var href = xmlLinkNode.Attributes["href"].Value;
                        if (href.EndsWith("/index.html"))
                        {
                            var documentUrl = baseUrl + href.TrimStart('.');
                            var xmlUrl = documentUrl.Replace("/index.html", "/xml.zip");
                            xmlUrls.Add(xmlUrl);
                        }
                    }
                }
            }

            return xmlUrls;
        }

        private void DoWork()
        {
            List<string> urls = this.GetLinks();
            this.ProcessDownloadXml(urls);
        }
    }
}
