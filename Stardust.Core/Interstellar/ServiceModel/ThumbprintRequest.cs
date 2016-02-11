using System.Runtime.Serialization;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar.ServiceModel
{
    [DataContract]
    public class ThumbprintRequest : RequestBase
    {
        [DataMember]
        public string IsuerAddress { get; set; }
    }
}