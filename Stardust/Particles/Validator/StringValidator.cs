//
// stringvalidator.cs
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
using System.Globalization;

namespace Stardust.Particles.Validator
{
    public static class StringValidator
    {
        public static bool IsEmail(this string self)
        {
            return self.IsValid(RegexCollction.EmailValidator);
        }

        public static bool IsUrl(this string self)
        {
            return self.IsValid(RegexCollction.UriValidator);
        }

        public static bool IsIsbnNumber(this string self)
        {
            return self.IsValid(RegexCollction.IsbnValidator);
        }

        public static bool IsIPv4Adress(this string self)
        {
            return self.IsValid(RegexCollction.IpV4Validator);
        }

        public static bool IsIPv6Adress(this string self)
        {
            return self.IsValid(RegexCollction.IpV6Validator);
        }

        public static bool IsDateTime(this string self, CultureInfo provider=null)
        {
            if (provider.IsNull())
                provider = CultureInfo.InvariantCulture;
            if (self.IsNullOrEmpty())
                return false;
            DateTime dateTime;
            return DateTime.TryParse(self ,provider,DateTimeStyles.AllowWhiteSpaces, out dateTime);
        }
    }
}