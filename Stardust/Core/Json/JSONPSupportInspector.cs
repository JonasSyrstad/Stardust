//
// JSONPSupportInspector.cs
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
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using Stardust.Particles;

namespace Stardust.Core.Json
{
    [ExcludeFromCodeCoverage]
    sealed class JSONPSupportInspector : IDispatchMessageInspector
    {
        // Assume utf-8, note that Data Services supports
        // charset negotation, so this needs to be more
        // sophisticated (and per-request) if clients will 
        // use multiple charsets
        private static readonly Encoding Encoding = Encoding.UTF8;

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.ContainsKey("UriTemplateMatchResults"))
            {
                var httpmsg = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                var match = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];

                var format = match.QueryParameters["$format"];
                if ("json".Equals(format, StringComparison.InvariantCultureIgnoreCase))
                {
                    // strip out $format from the query options to avoid an error
                    // due to use of a reserved option (starts with "$")
                    match.QueryParameters.Remove("$format");

                    // replace the Accept header so that the Data Services runtime 
                    // assumes the client asked for a JSON representation
                    httpmsg.Headers["Accept"] = "application/json";

                    var callback = match.QueryParameters["$callback"];
                    if (callback.ContainsCharacters())
                    {
                        match.QueryParameters.Remove("$callback");
                        return callback;
                    }
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (correlationState != null && correlationState is string)
            {
                // if we have a JSONP callback then buffer the response, wrap it with the
                // callback call and then re-create the response message
                var callback = (string)correlationState;

                var reader = reply.GetReaderAtBodyContents();
                reader.ReadStartElement();
                var content = Encoding.GetString(reader.ReadContentAsBase64());

                content = callback + "(" + content + ")";

                var newreply = Message.CreateMessage(MessageVersion.None, "", new Writer(content));
                newreply.Properties.CopyProperties(reply.Properties);

                reply = newreply;
            }
        }

        #endregion
        [ExcludeFromCodeCoverage]
        sealed class  Writer : BodyWriter
        {
            private readonly string Content;

            public Writer(string content)
            : base(false)
            {
                Content = content;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Binary");
                var buffer = Encoding.GetBytes(Content);
                writer.WriteBase64(buffer, 0, buffer.Length);
                writer.WriteEndElement();
            }
        }
    }
}