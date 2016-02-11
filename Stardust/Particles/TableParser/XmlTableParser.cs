//
// XmlTableParser.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Stardust.Clusters;

namespace Stardust.Particles.TableParser
{
    public class XmlTableParser : DynamicConfigurableObjectBase, ITableParser
    {
        public XmlTableParser()
            : base(ConfigurePropertyDefinitions())
        {
        }

        [ExcludeFromCodeCoverage]
        public ITableParser Initialize(string delimiter, string rowBreak, string textQualifier, bool isFirstRowHeaders)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetDelimiter(string delimiter)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetRowBreak(string rowBreak)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetTextQualifier(string textQualifier)
        {
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetHeaderRow(bool isFirstRowHeaders)
        {
            return this;
        }

        public Document Parse(string fileName)
        {
            var xml = new XmlDocument();
            xml.LoadXml(EncodingFactory.ReadFileText(fileName));
            return ParseXmlDocument(xml);
        }

        private Document ParseXmlDocument(XmlDocument xml)
        {
            var doc = new Document();
            foreach (XmlNode childNode in GetRoot(xml))
            {
                if (!doc.HaveHeader)
                    doc.SetHeaders(ExtractHeaderInformation(childNode, doc));
                doc.Add(GetRow(childNode, doc));
            }
            return doc;
        }

        private XmlNodeList GetRoot(XmlDocument xml)
        {
            if (GetPropertyValue<string>("RootName").IsNullOrEmpty())
                return xml.ChildNodes[1].ChildNodes;
            return xml.GetElementsByTagName(GetPropertyValue<string>("RootName"))[0].ChildNodes;
        }

        private static DocumentRow GetRow(XmlNode childNode, Document document)
        {

            var docRow = new DocumentRow(document);
            foreach (XmlNode xmlNode in childNode.ChildNodes)
            {
                docRow.Add(xmlNode.InnerText);
            }
            return docRow;
        }

        private static DocumentRow ExtractHeaderInformation(XmlNode childNode, Document document)
        {
            var docRow = new DocumentRow(document);
            foreach (XmlNode xmlNode in childNode.ChildNodes)
            {
                if (xmlNode.Attributes != null)
                {
                    var nameAttrib = xmlNode.Attributes.GetNamedItem("NAME");
                    if (nameAttrib.IsNull())
                        nameAttrib = xmlNode.Attributes.GetNamedItem("name");
                    if (nameAttrib.IsNull())
                        nameAttrib = xmlNode.Attributes.GetNamedItem("Name");
                    if (nameAttrib != null)
                    {
                        docRow.Add(nameAttrib.InnerText);

                    }
                }
                else
                    docRow.Add(xmlNode.Name);
            }
            return docRow;
        }

        [ExcludeFromCodeCoverage]
        public Document Parse(Stream stream, bool buffered = false)
        {
            var xml = new XmlDocument();
            xml.Load(stream);
            return ParseXmlDocument(xml);
        }

        [ExcludeFromCodeCoverage]
        public Document Parse(byte[] buffer, bool buffered = false)
        {
            using (var stream = new MemoryStream(buffer, false))
            {
                var xml = new XmlDocument();
                xml.Load(stream);
                return ParseXmlDocument(xml);
            }
        }

        private static Dictionary<string, Type> ConfigurePropertyDefinitions()
        {
            return new Dictionary<string, Type> { { "RootName", typeof(string) } };
        }


        public bool IsSuitableForFile(string fileName)
        {
            return fileName.EndsWithCaseInsensitive("xml");
        }

        public Parsers GetParserType()
        {
            return Parsers.SimpleXmlParser;
        }
    }
}