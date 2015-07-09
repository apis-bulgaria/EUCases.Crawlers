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

namespace ServiceEUCases.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ServiceEUCases.Data;

    public class DbRepository : IRepository
    {
        private static object lockObject = new object();

        public int GetOrCreateCrawlerId(string crawlerName)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                lock (lockObject)
                {
                    var crawler = (from c in context.Crawlers
                                   where c.CrawlerName == crawlerName
                                   select c.CrawlerId).FirstOrDefault();

                    if (crawler == 0)
                    {
                        Crawler newCrawler = new Crawler();
                        newCrawler.CrawlerName = crawlerName;
                        context.Crawlers.Add(newCrawler);
                        context.SaveChanges();

                        return this.GetOrCreateCrawlerId(crawlerName);
                    }

                    return crawler;
                }
            }
        }

        public string GetIdentifier(string identifier)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                var documentGroupIdentifier = (from d in context.DocumentGroups
                                               where d.Identifier == identifier
                                               select d.Identifier).FirstOrDefault();

                return documentGroupIdentifier;
            }
        }

        public int DeleteDocumentGroup(string identifier)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                return context.p_DeleteDocumentGroup(identifier);
            }
        }

        public void AddDocumentGroup(DocumentGroup newDocumentGroup)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                context.DocumentGroups.Add(newDocumentGroup);
                context.SaveChanges();
            }
        }

        public void AddNewLog(CrawlerLog crawlerLog)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                context.CrawlerLogs.Add(crawlerLog);
                context.SaveChanges();
            }
        }

        public void AddNewLogRange(IEnumerable<CrawlerLog> crawlerLogs)
        {
            using (var context = new CrawlerFrameworkEntities())
            {
                context.CrawlerLogs.AddRange(crawlerLogs);
                context.SaveChanges();
            }
        }
    }
}
