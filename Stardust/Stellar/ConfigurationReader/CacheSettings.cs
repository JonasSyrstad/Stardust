//
// CacheSettings.cs
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

using System.ComponentModel;
using Stardust.Particles;

namespace Stardust.Stellar.ConfigurationReader
{
    public class CacheSettings
    {
        public string CacheType
        {
            get
            {
                if (CacheImplementation.IsNullOrWhiteSpace()) return "default";
                return CacheImplementation;
            }
        }

        public string CacheImplementation { get; set; }


        public bool NotifyOnChange { get; set; }
        public string CacheName { get; set; }

        [DisplayName("Machine names")]
        public string MachineNames { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string PassPhrase { get; set; }

        public string SecurityMode { get; set; }

        public string ProtectionLevel { get; set; }
    }
}