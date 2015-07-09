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
    using System.Collections.Generic;
    using System.IO;
    using MetainfoEUCases;

    /// <summary>
    /// Base Crawler for all other crawlers.
    /// Implemet event fireing functionality
    /// </summary>
    public abstract class BaseCrawler : ICrawler
    {
        private object objectLock = new object();

        /// <summary>
        /// Implements EventHandler OnDocDownloaded
        /// see <see cref="ICrawler"/> interface
        /// </summary>
        public event EventHandler OnDocDownloaded
        {
            add
            {
                lock (this.objectLock)
                {
                    this.DocDownloadedEvent += value;
                }
            }

            remove
            {
                lock (this.objectLock)
                {
                    this.DocDownloadedEvent -= value;
                }
            }
        }

        private event EventHandler DocDownloadedEvent;

        /// <summary>
        /// starts the crawling process
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Take as a value well formatted Uri 
        /// and returns in the form of document format MIME
        /// </summary>
        /// <param name="link">Well formated Uri</param>
        /// <returns>Internet media type format</returns>
        public string GetDataFormat(string link)
        {
            string dataFormat = Path.GetExtension(link);

            switch (dataFormat)
            {
                case ".zip": return "application/zip";
                case ".gzip": return "application/gzip";
                case ".pdf": return "application/pdf";
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpg";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".tiff": return "image/tiff";
                case ".css": return "text/css";
                case ".html": return "text/html";
                case ".htm": return "text/htm";
                case ".js": return "text/javascript";
                case ".xml": return "text/xml";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                default: return "application/error";
            }
        }

        /// <summary>
        /// Take for value file or directory 
        /// and return the name of the file without extension
        /// </summary>
        /// <param name="path">File or Directory path information</param>
        /// <returns></returns>
        public virtual string GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public virtual string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public virtual string ClearFileName(string fileName)
        {
            fileName = fileName
                .Replace("/", string.Empty)
                .Replace("\\", string.Empty)
                .Replace("*", string.Empty)
                .Replace("<", string.Empty)
                .Replace(">", string.Empty)
                .Replace(":", string.Empty)
                .Replace("\"", string.Empty)
                .Replace("?", string.Empty)
                .Replace("|", string.Empty)
                .Replace("=", string.Empty)
                .Replace("amp;", string.Empty)
                .Replace("#", string.Empty)
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("@", string.Empty);

            fileName = fileName.Trim().ToLower();

            return fileName;
        }

        /// <summary>
        /// This method is called for every download document from extenders
        /// </summary>
        /// <param name="document">List of DocumentMeatInfo objects</param>
        /// <param name="folder">Download folder for document</param>
        protected void DoOnDocDownloaded(List<XmlDocumentMetaInfo> document, string folder)
        {
            lock (this.objectLock)
            {
                try
                {
                    EventHandler handler = this.DocDownloadedEvent;
                    if (handler != null)
                    {
                        handler(this, new DocDownloadedEventArguments(document, folder));
                    }
                }
                catch (Exception ex)
                {
                    CrawlerLog.LogException(ex);
                }
            }
        }
    }
}
