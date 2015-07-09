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

namespace ServiceEUCases.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Data;
    using Repositories;

    public static class LockHelper
    {
        private static object locker;

        private static bool checkForLog;

        private static string logFile;

        private static DbRepository db;

        static LockHelper()
        {
            locker = new object();
            checkForLog = true;
            logFile = ConfigurationManager.AppSettings["LogFile"];
            db = new DbRepository();
        }

        public static void WriteToFileLog(CrawlerLog crawlerLog)
        {
            lock (locker)
            {
                checkForLog = true;
                StreamWriter sw = new StreamWriter(logFile, true);
                using (sw)
                {
                    string logLine = (crawlerLog.IpAddress + " _^sep^_ " +
                                     crawlerLog.LogDate + " _^sep^_ " +
                                     crawlerLog.MetaXml + " _^sep^_ " +
                                     crawlerLog.ZipLength + " _^sep^_ " +
                                     crawlerLog.Identifier + " _^sep^_ " +
                                     crawlerLog.CrawlerName + " _^sep^_ " +
                                     crawlerLog.IsSuccess + " _^sep^_ " +
                                     crawlerLog.Operation + " _^sep^_ " +
                                     crawlerLog.Error).Replace("\r", " _^r^_ ").Replace("\n", " _^n^_ ");
                    sw.WriteLine(logLine);
                }
            }
        }

        public static void PushLocalLogToDb()
        {
            lock (locker)
            {
                if (checkForLog == true)
                {
                    if (File.Exists(logFile))
                    {
                        List<CrawlerLog> crawlerLogs = new List<CrawlerLog>();
                        StreamReader sr = new StreamReader(logFile);
                        using (sr)
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                crawlerLogs.Add(GetLog(line));

                                if (crawlerLogs.Count > 300)
                                {
                                    db.AddNewLogRange(crawlerLogs);
                                    crawlerLogs.Clear();
                                }
                            }
                        }

                        if (crawlerLogs.Count > 0)
                        {
                            db.AddNewLogRange(crawlerLogs);
                        }

                        File.Delete(logFile);
                    }

                    checkForLog = false;
                }
            }
        }

        private static CrawlerLog GetLog(string log)
        {
            log = log.Replace(" _^r^_ ", "\r").Replace(" _^n^_ ", "\n");
            var logAtribs = log.Split(new string[] { " _^sep^_ " }, StringSplitOptions.None);
            CrawlerLog crawlerLog = new CrawlerLog();
            crawlerLog.IpAddress = logAtribs[0].Trim() == string.Empty ? null : logAtribs[0].Trim();
            DateTime logDate = new DateTime();
            if (DateTime.TryParse(logAtribs[1].Trim(), out logDate))
            {
                crawlerLog.LogDate = logDate;
            }

            crawlerLog.MetaXml = logAtribs[2].Trim() == string.Empty ? null : logAtribs[2].Trim();
            int zipLength = 0;
            if (int.TryParse(logAtribs[3].Trim(), out zipLength))
            {
                crawlerLog.ZipLength = zipLength;
            }

            crawlerLog.Identifier = logAtribs[4].Trim() == string.Empty ? null : logAtribs[4].Trim();
            crawlerLog.CrawlerName = logAtribs[5].Trim() == string.Empty ? null : logAtribs[5].Trim();
            bool isSuccess = false;
            if (bool.TryParse(logAtribs[6].Trim(), out isSuccess))
            {
                crawlerLog.IsSuccess = isSuccess;
            }

            int operation = 0;
            if (int.TryParse(logAtribs[7].Trim(), out operation))
            {
                crawlerLog.Operation = operation;
            }

            crawlerLog.Error = logAtribs[8].Trim();

            return crawlerLog;
        }
    }
}