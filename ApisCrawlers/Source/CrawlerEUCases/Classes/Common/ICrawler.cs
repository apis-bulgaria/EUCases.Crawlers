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

    /// <summary>
    /// Defines methods and events wich all crawlers must support
    /// </summary>
    public interface ICrawler
    {
        /// <summary>
        /// Every crawler must fire this event when the document is downloaded completely
        /// </summary>
        event EventHandler OnDocDownloaded;

        /// <summary>
        /// Starts the crawling process
        /// </summary>
        void Start();
    }
}
