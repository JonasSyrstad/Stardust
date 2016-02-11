using System.Runtime.Serialization;
using Stardust.Core.Security;

namespace Stardust.Interstellar.ServiceModel
{
    [DataContract]
    public class ThumbprintUpdateRequest : ThumbprintRequest
    {
        

        [DataMember]
        public ThumbprintItem Thumbprint { get; set; }
    }
}