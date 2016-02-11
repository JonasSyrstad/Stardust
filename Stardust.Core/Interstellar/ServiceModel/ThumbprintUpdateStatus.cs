using System.Runtime.Serialization;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar.ServiceModel
{
    [DataContract]
    public class ThumbprintUpdateStatus : ResponseBase
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}