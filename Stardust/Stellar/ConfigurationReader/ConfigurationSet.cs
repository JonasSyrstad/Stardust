//
// ConfigurationSet.cs
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

namespace Stardust.Stellar.ConfigurationReader
{
    public class ConfigurationSet : ICloneable
    {
        public long Id { get; set; }

        public string SetName { get; set; }

        public string ParentSet { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime Created { get; set; }

        public virtual List<ConfigParameter> Parameters { get; set; }

        public virtual List<ServiceConfig> Services { get; set; }

        public virtual List<EnvironmentConfig> Environments { get; set; }

        public virtual List<EndpointConfig> Endpoints { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public string RequestedBy { get; set; }

        public string ReaderKey { get; set; }

        public bool AllowMasterKeyAccess { get; set; }

        public string ETag { get; set; }
    }
}