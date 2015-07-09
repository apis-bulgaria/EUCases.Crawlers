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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using Crawler.Classes.CRCompareXml;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public enum LawCategory
    {
        // Primary Legislation
        Ukpga,
        Ukla,
        Asp,
        Anaw,
        Mwa,
        Ukcm,
        Nia,
        Aosp,
        Aep,
        Aip,
        Apgb,
        Nisi,
        Mnia,
        Apni,

        // Secondary Legislation
        Uksi,
        Wsi,
        Ssi,
        Nisr,
        Ukci,
        Ukmo,
        Uksro,

        // Draft Legislation
        Ukdsi,
        Sdsi,
        Nidsr,

        // Impact Assessments
        Ukia
    }

    public class Legislation : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            foreach (LawCategory lawCategory in (LawCategory[])Enum.GetValues(typeof(LawCategory)))
            {
                this.DownloadLawsByYearRange(lawCategory, 1600, DateTime.Now.Year);
            }
        }

        private void DownloadLawsByYear(LawCategory category, int year)
        {
            this.DownloadLawsByYearRange(category, year, year);
        }

        private void DownloadLawsByYearRange(LawCategory category, int fromYear, int toYear)
        {
            string baseUrl = "http://www.legislation.gov.uk";
            string categoryUrl = this.GetUrlByCategory(category);
            HashSet<string> urls = new HashSet<string>();

            Parallel.For(
                fromYear, 
                toYear, 
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism }, 
                year =>
            {
                string rootUrl = categoryUrl + "/" + year;

                try
                {
                    HtmlWeb web = new HtmlWeb();
                    bool isNext = true;
                    do
                    {
                        HtmlDocument doc = web.Load(rootUrl);
                        var links = doc.DocumentNode.SelectNodes("//div[@id='content']//tbody//a");
                        if (links != null)
                        {
                            foreach (var link in links)
                            {
                                string navigateLink = link.Attributes["href"].Value;
                                navigateLink = navigateLink.Replace("/contents/enacted", string.Empty).Replace("/contents", string.Empty);
                                urls.Add(baseUrl + navigateLink);
                            }
                        }
                        else
                        {
                            isNext = false;
                        }

                        var pager = doc.DocumentNode.SelectSingleNode("//div[@class='prevPagesNextNav']//li[@class='pageLink next']//a");
                        if (pager != null)
                        {
                            rootUrl = baseUrl + pager.Attributes["href"].Value;
                        }
                        else
                        {
                            isNext = false;
                        }
                    } 
                    while (isNext);
                }
                catch (Exception ex)
                {
                    CrawlerLog.LogException(ex, rootUrl);
                }
            });

            Parallel.ForEach(
                   urls,
                   new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                   url =>
                   {
                       this.DownloadHtml(url);
                   });
        }

        private void DownloadHtml(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();
                    url = url + "/data.xml";
                    byte[] dataContent = client.DownloadData(url);
                    var md5Name = MD5HashHelper.GetMd5Hash(Encoding.UTF8.GetBytes(url));
                    string fileName = this.ClearFileName(md5Name);

                    documentMetaInfo.Add(new XmlDocumentMetaInfo()
                    {
                        File = fileName + ".xml",
                        Url = url,
                        Format = this.GetDataFormat(url),
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
        }

        private Dictionary<int, int> GetCountByYear(string url, int fromYear, int toYear)
        {
            var yearCount = new Dictionary<int, int>();
            for (int i = fromYear; i <= toYear; i++)
            {
                using (var client = new WebClient())
                {
                    string html = client.DownloadString(url + "/" + i);
                    Regex reg = new Regex("from " + i + @" has returned (\d*) results\.");
                    Match m = reg.Match(html);
                    if (m.Success)
                    {
                        var countStr = m.Groups[1].Value;
                        int count = 0;
                        if (int.TryParse(countStr, out count))
                        {
                            if (count > 0)
                            {
                                yearCount.Add(i, count);
                            }
                        }
                    }
                }
            }

            return yearCount;
        }

        private string GetUrlByCategory(LawCategory category)
        {
            string baseUrl = "http://www.legislation.gov.uk/";

            switch (category)
            {
                case LawCategory.Ukpga:
                    return baseUrl + "ukpga";
                case LawCategory.Ukla:
                    return baseUrl + "ukla";
                case LawCategory.Asp:
                    return baseUrl + "asp";
                case LawCategory.Anaw:
                    return baseUrl + "anaw";
                case LawCategory.Mwa:
                    return baseUrl + "mwa";
                case LawCategory.Ukcm:
                    return baseUrl + "ukcm";
                case LawCategory.Nia:
                    return baseUrl + "nia";
                case LawCategory.Aosp:
                    return baseUrl + "aosp";
                case LawCategory.Aep:
                    return baseUrl + "aep";
                case LawCategory.Aip:
                    return baseUrl + "aip";
                case LawCategory.Apgb:
                    return baseUrl + "apgb";
                case LawCategory.Nisi:
                    return baseUrl + "nisi";
                case LawCategory.Mnia:
                    return baseUrl + "mnia";
                case LawCategory.Apni:
                    return baseUrl + "apni";
                case LawCategory.Uksi:
                    return baseUrl + "uksi";
                case LawCategory.Wsi:
                    return baseUrl + "wsi";
                case LawCategory.Ssi:
                    return baseUrl + "ssi";
                case LawCategory.Nisr:
                    return baseUrl + "nisr";
                case LawCategory.Ukci:
                    return baseUrl + "ukci";
                case LawCategory.Ukmo:
                    return baseUrl + "ukmo";
                case LawCategory.Uksro:
                    return baseUrl + "uksro";
                case LawCategory.Ukdsi:
                    return baseUrl + "ukdsi";
                case LawCategory.Sdsi:
                    return baseUrl + "sdsi";
                case LawCategory.Nidsr:
                    return baseUrl + "nidsr";
                case LawCategory.Ukia:
                    return baseUrl + "ukia";
                default:
                    return baseUrl;
            }
        }
    }
}
