//
// CsvTableParser.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Stardust.Clusters;
using Stardust.Core;

namespace Stardust.Particles.TableParser
{
    public class CsvTableTokenizer : IEnumerable<string>
    {
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CsvRowTokenizer:IEnumerable<string>
    {
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CsvTableParser : DynamicConfigurableObjectBase, ITableParser
    {
        protected string Delimiter;
        protected string RowBreak;
        protected string TextQualifier;
        protected bool IsFirstRowHeaders;

        public CsvTableParser()
            : base(null)
        {
            Delimiter = "|";
            RowBreak = Environment.NewLine;
            TextQualifier = "\"";
            IsFirstRowHeaders = true;
        }

        public ITableParser Initialize(string delimiter, string rowBreak, string textQualifier, bool isFirstRowHeaders)
        {
            Delimiter = delimiter;
            RowBreak = rowBreak;
            TextQualifier = textQualifier;
            IsFirstRowHeaders = isFirstRowHeaders;
            return this;
        }

        public virtual Document Parse(string fileName)
        {
            var content = EncodingFactory.ReadFileText(fileName);
            return ParseFileContent(content);
        }

        private Document ParseFileContent(string content)
        {
            var doc = new Document();
            var isFirst = true;
            var rows = content.Split(new[] { RowBreak }, StringSplitOptions.RemoveEmptyEntries);
            return ParseRows(rows, isFirst, doc);
        }

        private Document ParseRows(string[] rows, bool isFirst, Document doc)
        {
            foreach (var row in rows)
            {
                if (row.IsNullOrWhiteSpace())
                {
                    continue;
                }
                if (isFirst && IsFirstRowHeaders)
                {
                    SetHeader(doc, ParseRow(row, doc));
                }
                else
                {
                    AddRow(doc, ParseRow(row, doc));
                }
                isFirst = false;
            }
            return doc;
        }

        protected void AddRow(Document doc, DocumentRow row)
        {
            doc.Add(row);
        }

        protected void SetHeader(Document doc, DocumentRow row)
        {
            doc.SetHeaders(row);
        }

        protected DocumentRow ParseRow(string row, Document document)
        {
            var documentRow = new DocumentRow(document);
            string[] rowContent;
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.useRegexParsing") == "true")
            {
                var regex = @",(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))";
                var myRegex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
                rowContent = myRegex.Split(row);
            }
            else
                rowContent = row.Replace("\r", "").Split(Delimiter.ToCharArray());
            foreach (var collumn in GetCells(rowContent))
                AddColumn(documentRow, collumn);
            return documentRow;
        }

        protected static void AddColumn(DocumentRow documentRow, string collumn)
        {
            documentRow.Add(collumn);
        }

        private IEnumerable<string> GetCells(IEnumerable<string> lineCells)
        {
            var cells = new List<string>();
            var joinNexCell = false;
            foreach (var lineCell in lineCells)
            {
                if (joinNexCell)
                {
                    joinNexCell = !lineCell.EndsWithCaseInsensitive(TextQualifier);
                    cells[cells.Count() - 1] += Delimiter + RemoveTextQualifier(lineCell);
                    continue;
                }
                if (ShouldJoinCells(lineCell))
                    joinNexCell = true;
                cells.Add(RemoveTextQualifier(lineCell));
            }
            return cells;
        }

        private bool ShouldJoinCells(string lineCell)
        {
            return TextQualifier.ContainsCharacters()
                && (lineCell.StartsWithCaseInsensitive(TextQualifier)
                && !lineCell.EndsWithCaseInsensitive(TextQualifier));
        }

        private string RemoveTextQualifier(string lineCell)
        {
            if (TextQualifier.ContainsCharacters())
                return lineCell.Replace(TextQualifier, "");
            return lineCell;
        }

        [ExcludeFromCodeCoverage]
        public virtual Document Parse(Stream stream, bool buffered = false)
        {
            if (buffered)
            {
                Logging.DebugMessage("Reading file buffered. to avoid out of memory exceptions.");
                using (var reader = new StreamReader(stream))
                {
                    var doc = new Document();
                    var isFirst = true;
                    var row = reader.ReadLine();
                    while (row.ContainsCharacters())
                    {
                        ParseRows(new[] { row }, isFirst, doc);
                        isFirst = false;
                        row = reader.ReadLine();
                    }
                    return doc;
                }
            }
            var content = EncodingFactory.ReadFileText(stream);
            return ParseFileContent(content);
        }

        [ExcludeFromCodeCoverage]
        public virtual Document Parse(byte[] buffer, bool buffered = false)
        {
            if (buffered)
            {
                return Parse(new MemoryStream(buffer), true);
            }
            var content = EncodingFactory.ReadFileText(buffer);
            return ParseFileContent(content);
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetDelimiter(string delimiter)
        {
            Delimiter = delimiter;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetRowBreak(string rowBreak)
        {
            RowBreak = rowBreak;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetTextQualifier(string textQualifier)
        {
            TextQualifier = textQualifier;
            return this;
        }

        [ExcludeFromCodeCoverage]
        public ITableParser SetHeaderRow(bool isFirstRowHeaders)
        {
            IsFirstRowHeaders = isFirstRowHeaders;
            return this;
        }

        public bool IsSuitableForFile(string fileName)
        {
            return fileName.EndsWithCaseInsensitive("csv")
                || fileName.EndsWithCaseInsensitive("txt");
        }

        public Parsers GetParserType()
        {
            return Parsers.Delimitered;
        }
    }
}