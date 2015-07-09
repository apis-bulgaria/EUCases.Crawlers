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

namespace ServiceEUCases.Test.Classes
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using MetainfoEUCases;

    public static class XmlHelper
    {
        public static UploadDocumentGroup GetUploadDocumentGroup(XmlDocumentGroup xmlDocumentGroup, string zipPath)
        {
            var docArray = File.ReadAllBytes(zipPath);

            UploadDocumentGroup uploadDocumentGroup = new UploadDocumentGroup();
            uploadDocumentGroup.Data = docArray;
            XmlSerializer serializer = new XmlSerializer(typeof(XmlDocumentGroup));
            using (TextWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, xmlDocumentGroup);
                uploadDocumentGroup.MetaInfo = writer.ToString();
            }

            return uploadDocumentGroup;
        }

        public static XmlDocumentGroup GetXmlDocumentGroup(string xmlPath)
        {
            string metainfo = File.ReadAllText(xmlPath);

            XmlDocumentGroup xmlDocumentGroup = null;
            XmlSerializer deserializer = new XmlSerializer(typeof(XmlDocumentGroup));
            using (TextReader textReader = new StringReader(metainfo))
            {
                xmlDocumentGroup = (XmlDocumentGroup)deserializer.Deserialize(textReader);
            }

            return xmlDocumentGroup;
        }
    }
}
