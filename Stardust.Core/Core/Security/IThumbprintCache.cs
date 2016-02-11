using Stardust.Interstellar.ServiceModel;
using Stardust.Particles;
using Stardust.Interstellar;

namespace Stardust.Core.Security
{
    public interface IThumbprintCache
    {
        void CacheThumbprint(ThumbprintItem thumbprint, string issuerAddress);

        ThumbprintItem GetChacedThumbprint(string issuerAddress);

    }

    class ThumbprintCachService : IThumbprintCache
    {
        public void CacheThumbprint(ThumbprintItem thumbprint, string issuerAddress)
        {
            using (var container=RuntimeFactory.CreateRuntime().CreateServiceProxy<IThumbprintCacheService>())
            {
                var result =
                    container.GetClient()
                        .UpdateThumbprint(new ThumbprintUpdateRequest
                        {
                            IsuerAddress = issuerAddress,
                            Thumbprint = thumbprint
                        });
                if(!result.Success)
                    Logging.DebugMessage("update thumbprint failed: "+result.Message);
            }
        }

        public ThumbprintItem GetChacedThumbprint(string issuerAddress)
        {
            using (var container = RuntimeFactory.CreateRuntime().CreateServiceProxy<IThumbprintCacheService>())
            {
                return container.GetClient().GetThumbprint(new ThumbprintRequest {IsuerAddress = issuerAddress}).Thumbprint;
            }
        }
    }
}