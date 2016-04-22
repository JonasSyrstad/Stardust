//
// Endpoint.cs
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel;

namespace Stardust.Interstellar.ConfigurationReader
{
    public class Endpoint
    {
        public Endpoint()
        {
            EndpointName = "secure";
            BindingType = "secure";
            MessageFormat = "Mtom";
            MaxMessageSize = 200000;
            TextEncoding = "Utf-8";
            MaxConnections = "200";
            MaxBufferPoolSize = 200000;
            MaxBufferSize = 200000;
            MaxReceivedSize = 200000;

        }
        [Key]
        public long Id { get; set; }

        [DefaultValue("secure")]
        public string EndpointName { get; set; }

        [DefaultValue("secure")]
        public string BindingType { get; set; }

        [DefaultValue("Mtom")]
        public string MessageFormat { get; set; }

        [DefaultValue(200000)]
        public long MaxMessageSize { get; set; }

        public string HostNameComparisonMode { get; set; }

        [DefaultValue("UTF-8")]
        public string TextEncoding { get; set; }

        public string Address { get; set; }

        [Obsolete("hide from swagger", false)]
        public virtual EndpointConfig Parent { get; set; }

        [DefaultValue(200)]
        public string MaxConnections { get; set; }

        public string Durable { get; set; }

        [DefaultValue(200000)]
        public long MaxReceivedSize { get; set; }

        [DefaultValue(200000)]
        public long MaxBufferPoolSize { get; set; }

        [DefaultValue(200000)]
        public int MaxBufferSize { get; set; }


        [Obsolete("hide from swagger", false)]
        public System.Xml.XmlDictionaryReaderQuotas ReaderQuotas { get; set; }

        [Obsolete("hide from swagger", false)]
        public TransferMode TransferMode { get; set; }

        public bool ExactlyOnce { get; set; }

        [DefaultValue(100)]
        public int CloseTimeout { get; set; }

        [DefaultValue(100)]
        public int OpenTimeout { get; set; }

        [DefaultValue(100)]
        public int ReceiveTimeout { get; set; }

        [DefaultValue(100)]
        public int SendTimeout { get; set; }

        public bool Deleted { get; set; }

        public string IssuerName { get; set; }

        public string IssuerActAsAddress { get; set; }

        public string IssuerMetadataAddress { get; set; }

        public string Audience { get; set; }

        public string Thumbprint { get; set; }

        public string CertificateValidationMode { get; set; }

        public bool OverrideSslSecurity { get; set; }

        public string IssuerAddress { get; set; }

        public string StsAddress { get; set; }

        public bool Ignore { get; set; }

        public Dictionary<string, string> PropertyBag { get; set; }
    }
}
