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

namespace Crawler.Classes.Crawlers.DE
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Bundesarbeitsgericht : BundesCommon
    {
        public override void Start()
        {
            string startUrl = "http://juris.bundesarbeitsgericht.de";
            string searchUrl = "/cgi-bin/rechtsprechung/list.py?Gericht=bag&Art=en&Datum=";
            this.Download(startUrl, searchUrl);
        }
    }
}
