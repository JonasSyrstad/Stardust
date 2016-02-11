//
// documentrow.cs
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
using System.Dynamic;

namespace Stardust.Particles.TableParser
{
    public class DocumentRow : DynamicObject, IEnumerable<string>
    {
        /// <summary>
        /// Returns the enumeration of all dynamic member names. 
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {

            if (Parent.HaveHeader) return Parent.Header.Columns;
            var list = new List<string>();
            for (var i = 0; i < Columns.Count; i++)
            {
                list.Add("column_" + i);
            }
            return list;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            try
            {
                if (Parent.HaveHeader)
                {
                    int? index;
                    if (TryGetColumnIndex(binder.Name, out index, binder.IgnoreCase))
                    {
                        result = this[index.Value];
                        return true;
                    }
                    result = null;
                    return false;
                }
                var name = binder.Name.Replace("column_", "");
                int pIndex;
                if (int.TryParse(name, out pIndex))
                {
                    result = this[pIndex];
                    return true;
                }
                result = null;
                return false;
            }
            catch (Exception ex)
            {

                throw new ArgumentException(string.Format("Cannot find item: {0}", binder.Name), "binder");
            }
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Parent.HaveHeader)
            {
                int? index;
                if (!TryGetColumnIndex(binder.Name, out index, binder.IgnoreCase))
                {
                    return false;
                }
                Columns[index.Value] = value.ToString();
                return true;
            }
            var name = binder.Name.Replace("column_", "");
            int pIndex;
            if (!int.TryParse(name, out pIndex)) return false;
            Columns[pIndex] = value.ToString();
            return true;
        }

        private const string NoHeaderDefined = "No header defined";
        private readonly List<string> Columns = new List<string>();

        public DocumentRow()
        {

        }

        internal DocumentRow(Document parent)
        {
            Parent = parent;
        }

        internal void SetParent(Document parent)
        {
            Parent = parent;
        }

        internal void Add(string column)
        {
            Columns.Add(column);
        }

        public string this[int index]
        {
            get { return Columns[index]; }
        }

        public string this[string columnName]
        {
            get
            {
                if (!Parent.HaveHeader) throw new StardustCoreException(NoHeaderDefined);
                var index = Parent.Header.GetColumnIndex(columnName);
                return Columns[index];
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)Columns).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Columns).GetEnumerator();
        }

        private int GetColumnIndex(string columnName)
        {
            int? index;
            if (!TryGetColumnIndex(columnName, out index, false)) throw new KeyNotFoundException(string.Format("No column with name {0}", columnName));
            return index.Value;
        }

        private bool TryGetColumnIndex(string columnName, out int? index, bool ignoreCase)
        {
            if (columnName.IsNullOrEmpty())
            {
                index = null;
                return false;
            }
            index = Parent.Header.Columns.FindIndex(p => p.Equals(columnName, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
            return true;
        }

        private Document Parent { get; set; }
    }
}