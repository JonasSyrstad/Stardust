//
// StringExtensions.cs
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Stardust.Clusters;
using Stardust.Particles.Collection;

namespace Stardust.Particles
{
    public static class StringExtensions
    {
        public static bool StartsWithInvariantCulture(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.StartsWith(value, StringComparison.InvariantCulture);
        }

        public static bool EndsWithInvariantCulture(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.EndsWith(value, StringComparison.InvariantCulture);
        }

        public static bool EqualsInvariantCulture(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.Equals(value, StringComparison.InvariantCulture);
        }

        public static bool StartsWithCaseInsensitive(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EndsWithCaseInsensitive(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsCaseInsensitive(this string self, string value)
        {
            if (self.IsNullOrEmpty())
                return false;
            return self.Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static byte[] GetByteArray(this string self, EncodingType encoder = EncodingType.Utf8)
        {
            var enc = EncodingTypeFactory.CreateEncoder(encoder);
            return enc.GetBytes(self);
        }

        public static void WriteToFile(this string self, string filePath, EncodingType encoding = EncodingType.Utf8)
        {
            var binaryData = self.GetByteArray();
            binaryData.WriteToDisk(filePath);
        }

        /// <summary>
        /// Validates the string using regular expressions.
        /// See Pragma.Core.CrossCutting.RegexCollction for some common validators like email and url.
        /// Please feel free to come with more, and we will add them (or update/enhance)
        /// </summary>
        public static bool IsValid(this string self, string pattern)
        {
            if (self.IsNullOrEmpty())
                return false;
            if (self.IsRegularExpression())
                return Regex.IsMatch(self, pattern);
            return false;
        }

        /// <summary>
        /// Searches the string for all occcuerances that mathces the regex pattern
        /// </summary>
        public static MatchCollection GetMatches(this string self, string pattern)
        {
            if (pattern.IsRegularExpression())
            {
                var rx = new Regex(pattern);
                return rx.Matches(self);
            }
            throw (new InvalidOperationException("Failed to evaluate expression"));
        }

        private static bool IsRegularExpression(this string pattern)
        {
            return pattern.ContainsCharacters() && (pattern.Trim().Length > 0);

        }

        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        public static bool ContainsCharacters(this string self)
        {
            return !self.IsNullOrEmpty();
        }

        public static bool StartsWithOneOfCaseInsensitive(this string self, params string[] conditions)
        {
            if (self.ValidateConditions(conditions))
                return conditions.Any(self.StartsWithCaseInsensitive);
            return false;
        }

        public static bool StartsWithOneOf(this string self, params string[] conditions)
        {
            if (self.ValidateConditions(conditions))
                return conditions.Any(self.StartsWithInvariantCulture);
            return false;
        }

        public static bool ContainsOneOf(this string self, params string[] conditions)
        {
            if (self.ValidateConditions(conditions))
                return conditions.Any(self.Contains);
            return false;
        }

        private static bool ValidateConditions(this string self, IEnumerable<string> conditions)
        {
            if (self.IsNullOrEmpty())
                return false;
            if (conditions.IsEmpty())
                return false;
            return true;
        }

        /// <summary>
        /// Equals one of the conditions. This method is culture invariant.
        /// </summary>
        public static bool EqualsOneOf(this string self, params string[] conditions)
        {
            if (self.ValidateConditions(conditions))
                return conditions.Any(self.EqualsInvariantCulture);
            return false;
        }

        public static string ToSeparatedString<T>(this IEnumerable<T> list, string separator = null)
        {
            separator = ValidateSeparator(separator);
            var listStr = new StringBuilder();
            foreach (var str in list)
            {
                AppendSeparatorIfApplicable(separator, listStr);
                listStr.Append(str);
            }
            return listStr.ToString();
        }

        private static string ValidateSeparator(string separator)
        {
            if (separator.IsNullOrEmpty())
                separator = ";";
            return separator;
        }

        private static void AppendSeparatorIfApplicable(string separator, StringBuilder toBuilder)
        {
            if (ShouldAppendSeparator(toBuilder))
                toBuilder.Append(separator);
        }

        private static bool ShouldAppendSeparator(StringBuilder toBuilder)
        {
            return toBuilder.Length > 0;
        }

        public static string RemoveTrailingZeroes(this string self)
        {
            if (!self.IsDecimal())
                return self;
            var ci = System.Globalization.CultureInfo.CurrentCulture;
            var comma = ci.NumberFormat.CurrencyDecimalSeparator;
            if (self.Contains(comma))
            {
                try
                {
                    self = self.TrimEnd('0');
                    self = self.TrimEnd(comma.ToCharArray());
                }
                catch { }
            }
            return self;
        }

        public static bool IsDecimal(this string self)
        {
            decimal value;
            return decimal.TryParse(self, out value);
        }

        public static string FormatString(this string self, params object[] args)
        {
            return String.Format(self, args);
        }

        public static void Append(this StringBuilder self, params string[] stringsToAppend)
        {
            foreach (var s in stringsToAppend)
            {
                self.Append(s);
            }
        }

        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
    }
}