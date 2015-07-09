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

namespace Crawler.Classes.CRCompareXml
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class MD5HashHelper
    {
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string GetMd5Hash(string filePath)
        {
            byte[] fileContent = File.ReadAllBytes(filePath);

            return GetMd5Hash(fileContent);
        }

        public static string GetMd5Hash(byte[] fileContent)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(fileContent);

                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
            }

            return sb.ToString();
        }
    }
}
