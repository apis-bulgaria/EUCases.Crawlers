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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using Ionic.Zip;
    using MetainfoEUCases;

    public abstract class LegislationCommon : BaseCrawler
    {
        protected abstract List<string> GetLinks();

        protected void ProcessDownloadXml(List<string> urls)
        {
            Parallel.ForEach(
                urls,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                url =>
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                            byte[] dataContent = client.DownloadData(url);

                            Dictionary<string, MemoryStream> zipContent = this.UnZipToMemory(dataContent);

                            foreach (var content in zipContent)
                            {
                                documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                {
                                    File = this.ClearFileName(content.Key),
                                    Url = url,
                                    Format = base.GetDataFormat(content.Key),
                                    Operation = Operation.None,
                                    DataContent = content.Value.ToArray()
                                });
                            }

                            if (documentMetaInfo.Count > 0)
                            {
                                string folder = Path.GetFileNameWithoutExtension(documentMetaInfo.FirstOrDefault().File).ToLower().Trim();
                                this.DoOnDocDownloaded(documentMetaInfo, folder);
                            }
                            else
                            {
                                CrawlerLog.LogInfo("Error: No zip content in url - " + url);
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        CrawlerLog.LogException(ex, url);
                    }
                });
        }

        protected void ProcessDownloadHtml(List<string> urls)
        {
            Parallel.ForEach(
                urls,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                url =>
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();

                            var html = client.DownloadString(url);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            var divNode = doc.GetElementbyId("content");
                            var anodes = divNode.SelectNodes("a");
                            if (anodes != null)
                            {
                                using (var exclient = new WebClient())
                                {
                                    foreach (var anode in anodes)
                                    {
                                        var exlink = anode.GetAttributeValue("href", string.Empty);
                                        exlink = exlink.Replace("amp;", string.Empty);
                                        var exMd5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(exlink));
                                        string exFileName = this.GetFileName(exMd5Name);
                                        byte[] exdataContent = exclient.DownloadData(exlink);

                                        documentMetaInfo.Add(new XmlDocumentMetaInfo()
                                        {
                                            File = exFileName + base.GetFileExtension(exlink),
                                            Url = exlink,
                                            Format = base.GetDataFormat(exlink),
                                            Operation = Operation.None,
                                            DataContent = exdataContent
                                        });
                                    }
                                }
                            }

                            string content = divNode.OuterHtml;
                            byte[] dataContent = Encoding.UTF8.GetBytes(content);
                            url = url.Replace("amp;", string.Empty);
                            var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url));
                            string fileName = this.GetFileName(md5Name);

                            documentMetaInfo.Add(new XmlDocumentMetaInfo()
                            {
                                File = fileName + ".html",
                                Url = url,
                                Format = "text/html",
                                Operation = Operation.None,
                                DataContent = dataContent
                            });

                            this.DoOnDocDownloaded(documentMetaInfo, fileName);
                        }
                    }
                    catch (WebException ex)
                    {
                        CrawlerLog.LogException(ex, url);
                    }
                });
        }

        private Dictionary<string, MemoryStream> UnZipToMemory(byte[] zipData)
        {
            Stream stream = new MemoryStream(zipData, 0, zipData.Length);
            var result = new Dictionary<string, MemoryStream>();
            using (ZipFile zip = ZipFile.Read(stream))
            {
                foreach (ZipEntry e in zip)
                {
                    MemoryStream data = new MemoryStream();
                    e.Extract(data);
                    result.Add(e.FileName, data);
                }
            }

            return result;
        }
    }
}
