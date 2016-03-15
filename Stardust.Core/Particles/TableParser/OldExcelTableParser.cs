//
// OldExcelTableParser.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Excel;
using Stardust.Core;

namespace Stardust.Particles.TableParser
{
    public class XlsTableParser : DynamicConfigurableObjectBase, ITableParser
    {
        public XlsTableParser()
            : base(null)
        { }

        [ExcludeFromCodeCoverage]
        public ITableParser Initialize(string delimiter, string rowBreak, string textQualifier, bool isFirstRowHeaders)
        {
            IsFirstRowHeaders = isFirstRowHeaders;
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

        public ITableParser SetHeaderRow(bool isFirstRowHeaders)
        {
            IsFirstRowHeaders = isFirstRowHeaders;
            return this;
        }

        public virtual Document Parse(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return Parse(stream);
        }

        protected virtual Document Parse(IExcelDataReader reader)
        {
            var doc = new Document();
            var isFirst = true;
            while (reader.Read())
            {
                if (isFirst && IsFirstRowHeaders)
                    doc.SetHeaders(ParseRow(reader, doc));
                else
                    doc.Add(ParseRow(reader, doc));
                isFirst = false;
            }
            return doc;
        }

        private static DocumentRow ParseRow(IExcelDataReader reader, Document parent)
        {
            var docRow = new DocumentRow(parent);
            for (var i = 0; i < reader.FieldCount; i++)
                docRow.Add(reader.GetString(i));
            return docRow;
        }

        [ExcludeFromCodeCoverage]
        public virtual Document Parse(Stream stream, bool buffered = false)
        {
            var excel = ExcelReaderFactory.CreateBinaryReader(stream);
            return Parse(excel);
        }

        [ExcludeFromCodeCoverage]
        public virtual Document Parse(byte[] buffer, bool buffered = false)
        {
            using (var stream = new MemoryStream(buffer))
            {
                //stream.Write(buffer, 0, buffer.Length);
                return Parse(stream);
            }
        }

        public bool IsFirstRowHeaders { get; set; }

        public virtual bool IsSuitableForFile(string fileName)
        {
            return fileName.EndsWithCaseInsensitive("xls");
        }

        public virtual Parsers GetParserType()
        {
            return Parsers.OldExcel;
        }
    }
}