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

namespace ServiceEUCases
{
    using System.Runtime.Serialization;
    using System.ServiceModel;

    /// <summary>
    /// Provides functionality for uploading single XmlDocumentGroup to Web service
    /// </summary>
    [ServiceContract]
    public interface IServiceEUCases
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="document">Coomplex type UploadDocumentGroup</param>
        /// <returns></returns>
        [OperationContract]
        string UploadFile(UploadDocumentGroup document);
    }
    
    /// <summary>
    /// Store information for uploaded XmlDocumentGroup
    /// </summary>
    [DataContract]
    public class UploadDocumentGroup
    {
        /// <summary>
        /// Xml description of XmlDocumentGroup
        /// </summary>
        [DataMember]
        public string MetaInfo { get; set; }
        
        /// <summary>
        /// Zip - archive containig all files 
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }
    }
}
