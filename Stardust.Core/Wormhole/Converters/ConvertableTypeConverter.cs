//
// convertabletypeconverter.cs
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
using System.ComponentModel;
using Stardust.Particles;

namespace Stardust.Wormhole.Converters
{
    class ConvertableTypeConverter : TypeConverterBase
    {
        /// <summary>
        /// Converts basic types to another basic type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected override object DoConvert(object source, object target = null)
        {
            try
            {
                if (source.IsNull()) return null;
                if (InType() == OutType()) return source;
                object destination;
                return TryConvert(source, out destination) ? destination : System.Convert.ChangeType(source, OutType());
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Cannot convert from {0} to {1}", InType(), OutType()), ex);
            }
        }

        private bool TryConvert(object source, out object destination)
        {
            var inn = InType();
            var oType = OutType();
            if (TypeExtensions.CanConvertTo(oType, inn))
            {
                destination = TypeDescriptor.GetConverter(InType()).ConvertTo(source, OutType());
                return true;
            }
            if (TypeExtensions.CanConvertFrom(oType, inn))
            {
                destination = TypeDescriptor.GetConverter(oType).ConvertFrom(source);
                return true;
            }
            destination = null;
            return false;
        }
    }
}