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
    using System.Linq;
    using MetainfoEUCases;

    /// <summary>
    /// Provides information for crawled Document
    /// </summary>
    public class DocDownloadedEventArguments : EventArgs
    {
        private List<XmlDocumentMetaInfo> document;
        private string _folder;

        /// <summary>
        /// Initiliaize class
        /// </summary>
        /// <param name="document">List of DocumentDocumentMetaInfo see XmlDocumentMetaInfo</param>
        /// <param name="folder">Folder where document is downloaded</param>
        public DocDownloadedEventArguments(List<XmlDocumentMetaInfo> document, string folder)
        {
            this.Document = document;
            this.Folder = folder;
        }

        /// <summary>
        /// Path to folder where document is downloaded
        /// </summary>
        public string Folder
        {
            get
            {
                return this._folder;
            }

            set
            {
                this._folder = value;
            }
        }

        /// <summary>
        /// All meta information for elemnts composing crawled document
        /// </summary>
        public List<XmlDocumentMetaInfo> Document
        {
            get
            {
                return this.document;
            }

            set
            {
                this.document = value;
            }
        }
    }
}
