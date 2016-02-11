using System.ServiceModel;

namespace Stardust.Interstellar.ServiceModel
{
    [ServiceContract(Namespace = "http://stardust.com/services/thumbprint/chache")]
    [ServiceName("ThumbprintCache","Stardust")]
    public interface IThumbprintCacheService
    {
        [OperationContract]
        ThumbprintResponse GetThumbprint(ThumbprintRequest request);

        [OperationContract]
        ThumbprintUpdateStatus UpdateThumbprint(ThumbprintUpdateRequest updateMessage);
    }
}
