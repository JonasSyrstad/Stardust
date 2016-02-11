//
// mappingdefinition.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using Stardust.Particles.TableParser;

namespace Stardust.Particles.TabularMapper
{

    public class MappingDefinition
    {
        internal List<FieldMapping> Fields1;

        public IEnumerable<FieldMapping> Fields
        {
            get { return Fields1; }
            set { Fields1 = value.ToList(); }
        }

        public static MappingDefinition CreateMappingDefinition()
        {
            return new MappingDefinition {Fields1 = new List<FieldMapping>()};
        }

        public MappingDefinition AddFieldMapping(int sourceColumnNumber, string targetMemberName, MemberTypes memberType)
        {
            Fields1.Add(new FieldMapping { MemberType = memberType, SourceColumnNumber = sourceColumnNumber, TargetMemberName = targetMemberName });
            return this;
        }

        public MappingDefinition AddFieldMapping(string sourceColumnName, string targetMemberName, MemberTypes memberType)
        {
            Fields1.Add(new FieldMapping { MemberType = memberType, SourceColumnName = sourceColumnName, TargetMemberName = targetMemberName });
            return this;
        }

        public MappingDefinition AddMappingFromDocumentWithHeaders(Document doc,MemberTypes memberType=MemberTypes.Property)
        {
            if(!doc.HaveHeader) throw new InvalidDataException("document does not have headers");
            foreach (var header in doc.Header)
            {
                AddFieldMapping(header, header, memberType);
            }
            return this;
        }
    }
}