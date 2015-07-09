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
    using System.Net;
    using System.Xml;

    public class LegislationVerwaltGovdata : LegislationCommon
    {
        public override void Start()
        {
            this.DoWork();
        }

        protected override List<string> GetLinks()
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (var client = new WebClient())
            {
                var xml = client.DownloadString("http://www.verwaltungsvorschriften-im-internet.de/vvii-toc.xml");
                xmlDoc.LoadXml(xml);
            }

            List<string> linksForDownload = new List<string>();
            foreach (XmlNode item in xmlDoc.SelectNodes("items//item"))
            {
                var uri = item.SelectSingleNode("link").InnerText;
                linksForDownload.Add(uri);
            }

            return linksForDownload;
        }

        private void DoWork()
        {
            List<string> urls = this.GetLinks();
            this.ProcessDownloadHtml(urls);
        }
    }
}
