﻿#region license
// Copyright (c) 2007-2009 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace SolrNet.Impl {
    /// <summary>
    /// Serializes a Solr document to xml
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public class SolrDocumentSerializer<T> : ISolrDocumentSerializer<T> {
        private readonly IReadOnlyMappingManager mappingManager;

        public SolrDocumentSerializer(IReadOnlyMappingManager mappingManager) {
            this.mappingManager = mappingManager;
        }

        /// <summary>
        /// Serializes a Solr document to xml
        /// </summary>
        /// <param name="doc">document to serialize</param>
        /// <returns>serialized document</returns>
        public XmlDocument Serialize(T doc) {
            var xml = new XmlDocument();
            var docNode = xml.CreateElement("doc");
            var fields = mappingManager.GetFields(typeof (T));
            foreach (var kv in fields) {
                var p = kv.Key;
                if (p.GetValue(doc, null) == null)
                    continue;
                var fieldNode = xml.CreateElement("field");
                var nameAtt = xml.CreateAttribute("name");
                nameAtt.InnerText = kv.Value;
                fieldNode.Attributes.Append(nameAtt);
                // TODO the following should be pluggable via SolrFieldSerializer or something similar
                if (p.PropertyType != typeof (string) && typeof (IEnumerable).IsAssignableFrom(p.PropertyType)) {
                    foreach (var o in p.GetValue(doc, null) as IEnumerable) {
                        fieldNode.InnerText = o.ToString();
                        docNode.AppendChild(fieldNode.CloneNode(true));
                    }
                    fieldNode = null;
                    //fieldNode.InnerXml = (GetPropertyValue(doc, p) ?? "").ToString();
                } else if (p.PropertyType == typeof (DateTime) || p.PropertyType == typeof (DateTime?)) {
                    fieldNode.InnerText = ((DateTime) p.GetValue(doc, null)).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                } else if (p.PropertyType == typeof (bool)) {
                    fieldNode.InnerText = p.GetValue(doc, null).ToString().ToLower();
                } else if (typeof (IFormattable).IsAssignableFrom(p.PropertyType)) {
                    var v = (IFormattable) p.GetValue(doc, null);
                    fieldNode.InnerText = v.ToString(null, CultureInfo.InvariantCulture);
                } else {
                    fieldNode.InnerText = p.GetValue(doc, null).ToString();
                }
                if (fieldNode != null)
                    docNode.AppendChild(fieldNode);
            }
            xml.AppendChild(docNode);
            return xml;
        }

        private static readonly IDictionary<Type, string> solrTypes;

        static SolrDocumentSerializer() {
            solrTypes = new Dictionary<Type, string>();
            solrTypes[typeof (int)] = "int";
            solrTypes[typeof (string)] = "str";
            solrTypes[typeof (bool)] = "bool";
            solrTypes[typeof (DateTime)] = "date";
        }
    }
}