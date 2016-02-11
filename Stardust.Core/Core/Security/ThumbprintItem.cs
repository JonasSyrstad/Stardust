using System;
using System.Runtime.Serialization;

namespace Stardust.Core.Security
{
    [DataContract]
    public class ThumbprintItem
    {
        [DataMember]
        public bool Resolved { get; set; }

        [DataMember]
        public string Thumbprint { get; set; }

        [DataMember]
        public DateTime Expiry { get; set; }
    }
}