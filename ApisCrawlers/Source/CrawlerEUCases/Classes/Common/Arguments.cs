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
    /// <summary>
    /// Provide information needed for managing crawling process
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Name of crawler
        /// </summary>
        public static string CrawlerName { get; set; }

        /// <summary>
        /// Destination folder that craler can use
        /// </summary>
        public static string DestinationFolder { get; set; }

        /// <summary>
        /// Folder for storing temporary data
        /// </summary>
        public static string TmpFolder { get; set; }

        /// <summary>
        /// Folder for storing crawled document
        /// </summary>
        public static string DataFolder { get; set; }

        /// <summary>
        /// Folder for storing crawled documents in database
        /// </summary>
        public static string SQLiteDataFile { get; set; }

        /// <summary>
        /// Storing database connection string
        /// </summary>
        public static string SQLiteConnectionString { get; set; }

        /// <summary>
        /// Language of crawled data
        /// </summary>
        public static string Language { get; set; }

        /// <summary>
        /// Defines the number of html pages that will be crawled simultaneously
        /// </summary>
        public static int MaxDegreeOfParallelism { get; set; }
    }
}
