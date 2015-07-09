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

namespace Crawler
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using Crawler.Classes.Common;

    public class Program
    {
        public static void Main(string[] args)
        {
            Arguments.DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
            Arguments.Language = ConfigurationManager.AppSettings["Language"];
            Arguments.MaxDegreeOfParallelism = int.Parse(ConfigurationManager.AppSettings["MaxDegreeOfParallelism"]);
            Arguments.CrawlerName = "Crawler.Classes.Crawlers." + ConfigurationManager.AppSettings["CrawlerName"];
            Arguments.SQLiteDataFile = Path.Combine(Arguments.DestinationFolder, "SQLiteData\\SQLiteCrawlerData.sqlite");
            var connectionString = ConfigurationManager.ConnectionStrings["SQLiteCrawlerDataEntities"];
            Arguments.SQLiteConnectionString = connectionString.ConnectionString.Replace("{SQLiteFile}", Arguments.SQLiteDataFile);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            CrawlerLog.LogInfo("Crawler: " + Arguments.CrawlerName + " start! " + DateTime.Now.ToString());
            Arguments.DataFolder = Arguments.DestinationFolder + "\\Data";
            Crawler.Classes.Common.CrawlerManager cm = new Crawler.Classes.Common.CrawlerManager();
            var type = Type.GetType(Arguments.CrawlerName);
            var currentCrawler = Activator.CreateInstance(type);

            var cc = (BaseCrawler)currentCrawler;
            cm.Start(cc);

            CrawlerLog.LogInfo("Crawler: " + Arguments.CrawlerName + " end! " + DateTime.Now.ToString() + " - Total time elapsed:" + sw.Elapsed);
        }
    }
}
