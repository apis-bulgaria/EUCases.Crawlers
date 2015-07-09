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

namespace Crawler.Classes.Crawlers.EU
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Crawler.Classes.Common;
    using Crawler.Classes.Crawlers.EU.HudocJson;
    using MetainfoEUCases;

    public class HudocEchr : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            // Service at http://hudoc.echr.coe.int/webservices/query/{COUNTRY}?q={QUERY}&s={SORT}&st={START}&c={COUNT}&f={SIMILAR}
            // Service at http://hudoc.echr.coe.int/webservices/query/{COUNTRY}/xml?q={QUERY}&s={SORT}&st={START}&c={COUNT}&t={TRANSFORM}&f={SIMILAR}
            // Service at http://hudoc.echr.coe.int/webservices/content/{COUNTRY}/body/html/{ITEMID}
            // Service at http://hudoc.echr.coe.int/webservices/content/{COUNTRY}/xml?i={ITEMID}&t={XSLFILENAME}
            // {COUNTRY} = ENG
            // {QUERY} = NOT (DOCTYPE=PR OR DOCTYPE=HFCOMOLD OR DOCTYPE=HECOMOLD) AND (languageisocode="ENG") AND (documentcollectionid="CASELAW")
            // {SIMILAR} = 1
            // &s=kpdate%20Descending - Sort by Date(Newest)
            // &c=500 - take count
            // &st=1 - start from
            string query = @"NOT%20(DOCTYPE%3DPR%20OR%20DOCTYPE%3DHFCOMOLD%20OR%20DOCTYPE%3DHECOMOLD)%20AND%20(languageisocode%3D%22ENG%22)%20AND%20(documentcollectionid%3D%22CASELAW%22)";
            bool isNewPage = true;
            int count = 1;
            int offoffset = 500;
            WebClient client = new WebClient();

            do
            {
                string url = "http://hudoc.echr.coe.int/webservices/query/eng?q=" + query + "&s=kpdate%20Descending&st=" + count + "&c=" + offoffset + "&f=1";
                count += offoffset;

                var jsonString = client.DownloadString(url);
                RootObject jsonResults = new JavaScriptSerializer().Deserialize<RootObject>(jsonString);

                if (jsonResults.results.Count > 0)
                {
                    Parallel.ForEach(
                                jsonResults.results,
                                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                                resultitem =>
                                {
                                    var itemId = resultitem.columns.ITEMID;

                                    try
                                    {
                                        List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                                        // Html Body
                                        WebClient clientHudoc = new WebClient();
                                        string bodyUrl = "http://hudoc.echr.coe.int/webservices/content/eng/body/html/" + itemId;
                                        string fileName = this.ClearFileName(itemId);
                                        var bodyData = clientHudoc.DownloadData(bodyUrl);
                                        documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                        {
                                            File = fileName + "_htmlbody.html",
                                            Url = bodyUrl,
                                            Format = "text/htm",
                                            Operation = Operation.None,
                                            DataContent = bodyData
                                        });

                                        // Meta Data
                                        string metadataUrl = "http://hudoc.echr.coe.int/webservices/content/eng/xml?i=" + itemId;
                                        var metadataData = clientHudoc.DownloadData(metadataUrl);
                                        documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                        {
                                            File = fileName + "_metadata.xml",
                                            Url = bodyUrl,
                                            Format = "text/xml",
                                            Operation = Operation.None,
                                            DataContent = metadataData
                                        });

                                        if (documentMetaInfo.Count > 0)
                                        {
                                            this.DoOnDocDownloaded(documentMetaInfo, fileName);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CrawlerLog.LogInfo(itemId);
                                        CrawlerLog.LogException(ex);
                                    }
                                });
                }
                else
                {
                    isNewPage = false;
                }
            }
            while (isNewPage);
        }
    }
}