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

namespace Crawler.Classes.Common
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// CrawlerLog class provide functionality for logging exceptions and information into file
    /// </summary>
    public class CrawlerLog
    {
        private static object objectLock = new object();

        private static string infoLog = "_debug_log.txt";

        /// <summary>
        /// Logging directory
        /// </summary>
        public static string LOG_DIR
        {
            get
            {
                return Path.Combine(Arguments.DestinationFolder, "logs");
            }
        }

        /// <summary>
        /// Name of log file 
        /// </summary>
        public static string InfoLog
        {
            get
            {
                return infoLog == null ? "_status.txt" : infoLog;
            }

            set
            {
                DateTime now = DateTime.Now;
                infoLog = value + "_" + string.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".log";
            }
        }

        /// <summary>
        /// Write url with exception along with innerexception layer
        /// </summary>
        /// <param name="exc">Raised exception</param>
        /// <param name="url">Url to be loged</param>
        public static void LogException(Exception exc, string url)
        {
            lock (objectLock)
            {
                CreateDirIfNecessary();

                using (FileStream ferr = new FileStream(LOG_DIR + "\\" + InfoLog, FileMode.Append))
                {
                    StreamWriter ferrors = new StreamWriter(ferr);
                    StringBuilder message = new StringBuilder();

                    message.Append("\nERROR! Time: ").Append(DateTime.Now);
                    if (!string.IsNullOrEmpty(url))
                    {
                        message.Append(" Url: " + url);
                    }

                    int innerExceptionLayer = 0;
                    while (null != exc)
                    {
                        if (innerExceptionLayer > 0)
                        {
                            message.Append("\nInner exception layer: " + innerExceptionLayer);
                        }

                        message.Append(" Message:").Append(exc.Message).Append('\n')
                            .Append("STACK TRACE:\n")
                            .Append(exc.StackTrace);

                        exc = exc.InnerException;
                        innerExceptionLayer++;
                    }

                    Console.WriteLine(message.ToString());
                    ferrors.Write(message.ToString());
                    ferrors.Flush();
                }
            }
        }

        /// <summary>
        /// Write given message into file
        /// </summary>
        /// <param name="msg"></param>
        public static void LogInfo(string msg)
        {
            lock (objectLock)
            {
                CreateDirIfNecessary();

                using (FileStream fstat = new FileStream(LOG_DIR + "\\" + infoLog, FileMode.Append))
                {
                    StreamWriter fstatus = new StreamWriter(fstat);
                    StringBuilder s = new StringBuilder();
                    s.Append("\nTime: ").Append(DateTime.Now);

                    s.Append("; Message: ").Append(msg);
                    Console.WriteLine(s.ToString());
                    fstatus.Write(s.ToString());
                    fstatus.Flush();
                }
            }
        }

        /// <summary>
        /// Write exception along with innerexception layer
        /// </summary>
        /// <param name="exc">Exception to be loged</param>
        public static void LogException(Exception exc)
        {
            LogException(exc, null);
        }

        internal static void LogError(string p)
        {
            LogException(null, p);
        }

        private static void CreateDirIfNecessary()
        {
            if (!Directory.Exists(LOG_DIR))
            {
                Directory.CreateDirectory(LOG_DIR);
            }
        }
    }
}