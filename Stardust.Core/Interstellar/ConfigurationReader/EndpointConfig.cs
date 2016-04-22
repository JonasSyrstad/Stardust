//
// EndpointConfig.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.ConfigurationReader
{
    public class EndpointConfig : ICloneable
    {
        private static bool DoFormat = true;
        public static void DoFormatOnRemoteAddress(bool doFormat)
        {
            DoFormat = doFormat;
        }

        [Key]
        public long Id { get; set; }

        public string ServiceName { get; set; }

        public string ActiveEndpoint { get; set; }

        public virtual List<Endpoint> Endpoints { get; set; }

        [Obsolete("hide from swagger", false)]
        public virtual ConfigurationSet Set { get; set; }

        public bool Deleted { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        internal string GetRemoteAddress(string root = null)
        {
            var active = GetEndpoint(ActiveEndpoint);//.Address;
            var part = active;
            var typeName = part.EndpointName;
            var adr = part.Address;
            var address = adr;
            if (adr.EndsWith(".svc") && ConfigurationManagerHelper.GetValueOnKey("stardust.AppendBindingType") == "true")
                address = adr + "/" + typeName;
            return Resolver.Activate<IEndpointUrlFormater>().GetEndpointAddress(address, root);
        }
        private readonly ConcurrentDictionary<string , Endpoint> endpointCache=new ConcurrentDictionary<string, Endpoint>(); 
        public Endpoint GetEndpoint(string name)
        {
            Endpoint endpoint;
            if (endpointCache.TryGetValue(name.ToLower(), out endpoint)) return endpoint;
            endpoint= (from e in Endpoints where string.Equals(e.EndpointName, name, StringComparison.OrdinalIgnoreCase) select e).FirstOrDefault();
            endpointCache.TryAdd(name.ToLower(), endpoint);
            return endpoint;
        }


    }
}