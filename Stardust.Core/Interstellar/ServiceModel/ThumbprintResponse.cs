using System.Runtime.Serialization;
using Stardust.Core.Security;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar.ServiceModel
{
    [DataContract]
    public class ThumbprintResponse : ResponseBase
    {
        [DataMember]
        public ThumbprintItem Thumbprint { get; set; }
    }
}