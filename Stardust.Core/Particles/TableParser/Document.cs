//
// document.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Stardust.Particles.TableParser
{
    [DataContract]
    public class Document : ValidateableDtoBase, IEnumerable<DocumentRow>
    {
        private List<DocumentRow> PrivateRows = new List<DocumentRow>();

        [DataMember]
        public DocumentRow Header { get; set; }

        public void RecreateStructure()
        {
            if (Header.IsInstance())
            {
                Header.SetParent(this);
                HaveHeader = true;
            }
            foreach (var row in Rows)
            {
                row.SetParent(this);
                if (!HasRows) HasRows = true;
            }
        }

        [DataMember]
        public IEnumerable<DocumentRow> Rows
        {
            get { return PrivateRows; }
            set
            {
                PrivateRows = value.ToList();
            }
        }

        public bool HaveHeader { get; private set; }

        public bool HasRows { get; private set; }

        public long NumberOfRows
        {
            get
            {
                if (HasRows)
                    return Rows.Count();
                return 0;
            }
        }

        public IEnumerator<DocumentRow> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        public override bool IsValid()
        {
            if (!HasRows) return true;
            var cellCount = from c in Rows select c.Count();
            if (cellCount.Distinct().Count() != 1) return false;
            if (HaveHeader)
                if (cellCount.First() != Header.Count())
                    return false;
            return true;
        }

        internal void SetHeaders(DocumentRow header)
        {
            Header = header;
            HaveHeader = true;
        }

        internal void Add(DocumentRow documentRow)
        {
            PrivateRows.Add(documentRow);
            HasRows = true;
        }

        protected void CreateNewRow(params string[] values)
        {
            var row = new DocumentRow(this);
            foreach (var value in values)
            {
                row.Add(value);
            }
            Add(row);
        }

        protected void CreateHeader(params string[] values)
        {
            var row = new DocumentRow(this);
            foreach (var value in values)
            {
                row.Add(value);
            }
            Header = row;
            HaveHeader = true;
        }

        public DocumentRow this[int index]
        {
            get { return PrivateRows[index]; }
        }
    }
}