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

namespace Crawler.Classes.Crawlers.FR
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Crawler.Classes.Common;
    using HtmlAgilityPack;
    using MetainfoEUCases;

    public class Legifrance : BaseCrawler
    {
        public override void Start()
        {
            this.DoWork();
        }

        private void DoWork()
        {
            // rechJuriConst.do?reprise=true&page=1&radioButton=contentieux&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champTitre=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriConst.do?reprise=true&page=1&radioButton=controle&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champTitre=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher

            // rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Tribunal+des+conflits&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Conseil+d%27Etat&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Cour+administrative+d%27appel&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Tribunal+administratif&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher

            // rechJuriJudi.do?reprise=true&page=1&champJuridictions=cour+de+cassation&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriJudi.do?reprise=true&page=1&champJuridictions=juridiction+d%27appel&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriJudi.do?reprise=true&page=1&champJuridictions=juridictions+du+premier+degre&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            // rechJuriJudi.do?reprise=true&page=1&champJuridictions=tribunal+des+conflits&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher
            string baseUrl = "http://www.legifrance.gouv.fr";
            List<string> postDataList = new List<string>() 
            {
               "/rechJuriConst.do?reprise=true&page=1&radioButton=contentieux&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champTitre=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriConst.do?reprise=true&page=1&radioButton=controle&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champTitre=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",

               "/rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Tribunal+des+conflits&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Conseil+d%27Etat&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Cour+administrative+d%27appel&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriAdmin.do?reprise=true&page=1&champJuridictions=Tribunal+administratif&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",

               "/rechJuriJudi.do?reprise=true&page=1&champJuridictions=cour+de+cassation&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriJudi.do?reprise=true&page=1&champJuridictions=juridiction+d%27appel&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriJudi.do?reprise=true&page=1&champJuridictions=juridictions+du+premier+degre&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher",
               "/rechJuriJudi.do?reprise=true&page=1&champJuridictions=tribunal+des+conflits&champNumero=&champDateDecision1J=&champDateDecision1M=&champDateDecision1A=&champMotsRecherches1=&champMotsRecherches2=&bouton=Rechercher"
            };

            foreach (var postData in postDataList)
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument docNavi = web.Load(baseUrl + postData);
                string navigation = string.Empty;
                var linkNode = docNavi.DocumentNode.SelectSingleNode("//ul[@class='pagination']//a");
                if (linkNode != null)
                {
                    if (linkNode.Attributes["href"] != null)
                    {
                        var naviString = linkNode.Attributes["href"].Value;
                        naviString = naviString.Replace("amp;", string.Empty);
                        navigation = baseUrl + naviString.Substring(0, naviString.Length - 1);
                    }
                }

                List<string> documentUrls = new List<string>();
                bool isNext = true;
                int pageNumber = 1;
                do
                {
                    HtmlDocument doc = web.Load(navigation + pageNumber);
                    pageNumber++;

                    var documentLinks = doc.DocumentNode.SelectNodes("//div[@id='center']//a");

                    if (documentLinks != null)
                    {
                        foreach (var documentLink in documentLinks)
                        {
                            if (documentLink.Attributes["href"] != null)
                            {
                                var docString = documentLink.Attributes["href"].Value;
                                docString = docString.Replace("amp;", string.Empty);
                                documentUrls.Add(baseUrl + docString);
                            }
                        }
                    }
                    else
                    {
                        if (doc.DocumentNode.InnerHtml.Contains("Résultats de votre recherche : La page de recherche demandée n&#39;existe pas"))
                        {
                            isNext = false;
                        }
                    }
                } 
                while (isNext);

                this.DownloadHtml(documentUrls);
            }
        }

        private void DownloadHtml(List<string> documentUrls)
        {
            Parallel.ForEach(
                documentUrls,
                new ParallelOptions { MaxDegreeOfParallelism = Arguments.MaxDegreeOfParallelism },
                documentUrl =>
                {
                    try
                    {
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument doc = web.Load(documentUrl);

                        var content = doc.DocumentNode.SelectSingleNode("//div[@id='content_false']");
                        List<XmlDocumentMetaInfo> documentMetaInfo = new List<XmlDocumentMetaInfo>();
                        string fileName = this.GetName(documentUrl);

                        if (content != null)
                        {
                            var infoContent = Encoding.UTF8.GetBytes(content.OuterHtml);

                            documentMetaInfo.Add(new XmlDocumentMetaInfo()
                            {
                                File = fileName.ToLower().Trim(),
                                Url = documentUrl,
                                Format = "text/html",
                                Operation = Operation.None,
                                DataContent = infoContent
                            });
                        }
                        else
                        {
                            throw new ArgumentNullException("Null content node: " + documentUrls);
                        }

                        if (documentMetaInfo.Count > 0)
                        {
                            this.DoOnDocDownloaded(documentMetaInfo, Path.GetFileNameWithoutExtension(fileName).ToLower().Trim());
                        }
                    }
                    catch (Exception ex)
                    {
                        CrawlerLog.LogException(ex, documentUrl);
                    }
                });
        }

        private string GetName(string documentUrl)
        {
            string fileName = "error.html";
            string pattern = "&idTexte=([\\w]*?)&fastReqId=";
            var reg = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = reg.Match(documentUrl);
            if (match.Success)
            {
                fileName = match.Groups[1].Value + ".html";
            }

            return fileName;
        }

        private string ReadFully(string documentUrl, string referer, string postData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            var request = (HttpWebRequest)HttpWebRequest.Create(documentUrl);
            request.ContentLength = byteArray.Length;
            request.Method = "POST";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Host = "www.legifrance.gouv.fr";
            request.Referer = "http://www.legifrance.gouv.fr/rechJuriConst.do?reprise=true&page=1";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // (request as HttpWebRequest).KeepAlive = false;
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    return UTF8Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
