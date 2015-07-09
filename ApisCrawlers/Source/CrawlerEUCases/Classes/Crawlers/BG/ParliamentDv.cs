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

    public class ParliamentDv : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            List<int> indexes = new List<int>(100000);

            for (int i = 10000; i < 100000; i++)
            {
                indexes.Add(i);
            }

            Parallel.ForEach(
                indexes,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                index =>
                {
                    string url = "http://dv.parliament.bg/DVWeb/showMaterialDV.jsp?idMat=" + index;
                    this.ProcessSignlePage(url, index);
                });
        }

        private void ProcessSignlePage(string url, int index)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);

                var contentDiv = doc.DocumentNode.SelectSingleNode("//div[@styleclass='parent']/table");

                if (contentDiv != null)
                {
                    // Root Document
                    var rootContent = Encoding.UTF8.GetBytes(contentDiv.OuterHtml);
                    string fileName = index.ToString();
                    List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = fileName,
                        Url = url,
                        Format = "text/htm",
                        Operation = Operation.None,
                        DataContent = rootContent
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
