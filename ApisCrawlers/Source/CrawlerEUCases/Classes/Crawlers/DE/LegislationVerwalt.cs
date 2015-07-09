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
    using HtmlAgilityPack;

    public class LegislationVerwalt : LegislationCommon
    {
        public override void Start()
        {
            this.DoWork();
        }

        protected override List<string> GetLinks()
        {
            string url = "http://www.verwaltungsvorschriften-im-internet.de/erlassstellen.html";
            string baseUrl = "http://www.verwaltungsvorschriften-im-internet.de";
            List<string> rootUrls = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//div[@id='container']//a");

            foreach (var rootNavigationLink in linkNodes)
            {
                if (rootNavigationLink.Attributes["href"] != null)
                {
                    var href = rootNavigationLink.Attributes["href"].Value;
                    rootUrls.Add(baseUrl + href.TrimStart('.'));
                }
            }

            List<string> xmlUrls = new List<string>();
            foreach (var htmlDocumentLink in rootUrls)
            {
                HtmlWeb rootWeb = new HtmlWeb();
                HtmlDocument rootDoc = rootWeb.Load(htmlDocumentLink);
                HtmlNodeCollection documentLinkNodes = rootDoc.DocumentNode.SelectNodes("//div[@id='container']//a");

                foreach (var documentLinkNode in documentLinkNodes)
                {
                    if (documentLinkNode.Attributes["href"] != null)
                    {
                        var href = documentLinkNode.Attributes["href"].Value;
                        xmlUrls.Add(baseUrl + href.TrimStart('.'));
                    }
                }
            }

            return xmlUrls;
        }

        private void DoWork()
        {
            List<string> urls = this.GetLinks();
            this.ProcessDownloadHtml(urls);
        }
    }
}
