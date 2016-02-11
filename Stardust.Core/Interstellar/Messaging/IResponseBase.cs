//
// IResponseBase.cs
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
using System.Runtime.Serialization;
using Stardust.Interstellar.Serializers;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.Messaging
{
    [DataContract(Namespace = NS)]
    public class ResponseHeader
    {
        internal const string NS = "http://stardustframework.com/interstellar/messaging/headers/response/";

        [DataMember]
        public long ExecutionTime { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string ReferingMessageId { get; set; }

        [DataMember]
        public DateTime? TimeStamp { get; set; }

        [DataMember]
        public string SupportCode { get; set; }

        /// <summary>
        /// Use this form .net code, and not SerializedCallStack
        /// </summary>
        [IgnoreDataMember]
        public CallStackItem CallStack
        {
            get
            {
                try
                {
                    if (SerializedCallStack.ContainsCharacters())
                        return Resolver.Activate<IReplaceableSerializer>().Deserialize(SerializedCallStack);
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    SerializedCallStack = Resolver.Activate<IReplaceableSerializer>().Serialize(value);
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, "Failed to serialize callstack");
                }
            }
        }

        [DataMember]
        public string RuntimeInstance
        {
            get;
            set;
        }

        [DataMember]
        public string OriginalRuntimeInstance { get; set; }


        [DataMember]
        public string ServerIdentity { get; set; }


        /// <summary>
        /// This is done to make call stack available over Rest, not to be used from code
        /// </summary>
        [DataMember]
        public string SerializedCallStack { get; set; }

    }
    /// <summary>
    /// Enables the Runtime to return call-stack information back to service clients.
    /// </summary>
    public interface IResponseBase
    {
        ResponseHeader ResponseHeader { get; set; }
        
    }
}