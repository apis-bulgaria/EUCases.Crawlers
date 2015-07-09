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

namespace Crawler.Classes.Crawlers.EU.HudocJson
{
    using System;
    using System.Collections.Generic;

    public class RootObject
    {
        public object bestbets { get; set; }

        public List<object> refinementvalues { get; set; }

        public int resultcount { get; set; }

        public List<Result> results { get; set; }

        public object specialterms { get; set; }

        public Spellingsuggestion spellingsuggestion { get; set; }
    }
}
