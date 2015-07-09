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

namespace ServiceEUCases.Classes
{
    public static class XsdSchema
    {
        public static string XsdDocumentGroup
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""windows-1251""?>
<xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns=""xsdDocumentgroup"" targetNamespace=""xsdDocumentgroup"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""documentgroup"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""1"" maxOccurs=""unbounded"" name=""document"">
          <xs:complexType>
            <xs:attribute name=""format"" use=""required"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
            <xs:attribute name=""file"" type=""xs:string"" use=""required"" />
            <xs:attribute name=""identifier"" use=""required"">
              <xs:simpleType >
                <xs:restriction base=""xs:string"">
                  <xs:pattern value=""[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}""/>
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
            <xs:attribute name=""operation"" use=""required"">
              <xs:simpleType >
                <xs:restriction base=""xs:string"">
                  <xs:enumeration value=""Add"" />
                  <xs:enumeration value=""Upd"" />
                  <xs:enumeration value=""Del"" />
                  <xs:enumeration value=""None"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
            <xs:attribute name=""url"" type=""xs:string"" use=""required""/>
            <xs:attribute name=""md5"" use=""required"" >
              <xs:simpleType >
                <xs:restriction base=""xs:string"">
                  <xs:pattern value=""[a-fA-F0-9]{32}""/>
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
      <xs:attribute name=""date"" type=""xs:dateTime"" use=""required"" />
      <xs:attribute name=""crawler"" type=""xs:string"" use=""required"" />
      <xs:attribute name=""lang"" type=""xs:string"" use=""required"" />
      <xs:attribute name=""format"" use=""required"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""application/zip"" />
            <xs:enumeration value=""application/gzip"" />
            <xs:enumeration value=""application/pdf"" />
            <xs:enumeration value=""image/gif"" />
            <xs:enumeration value=""image/jpeg"" />
            <xs:enumeration value=""image/png"" />
            <xs:enumeration value=""image/tiff"" />
            <xs:enumeration value=""image/bmp"" />
            <xs:enumeration value=""text/css"" />
            <xs:enumeration value=""text/html"" />
            <xs:enumeration value=""text/htm"" />
            <xs:enumeration value=""text/javascript"" />
            <xs:enumeration value=""text/xml"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""filename"" type=""xs:string"" use=""required"" />
      <xs:attribute name=""identifier"" use=""required"" >
        <xs:simpleType >
          <xs:restriction base=""xs:string"">
            <xs:pattern value=""[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}""/>
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
      <xs:attribute name=""operation"" use=""required"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Add"" />
            <xs:enumeration value=""Upd"" />
            <xs:enumeration value=""Del"" />
            <xs:enumeration value=""None"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:attribute>
    </xs:complexType>
  </xs:element>
</xs:schema>";
            }
        }
    }
}