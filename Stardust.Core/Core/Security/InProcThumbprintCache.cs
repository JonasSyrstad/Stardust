using System;
using Stardust.Particles;

namespace Stardust.Core.Security
{
    public class InProcThumbprintCache : IThumbprintCache
    {
        private const string StardustInternalThumbprint = "stardust.internal.Thumbprint";
        public void CacheThumbprint(ThumbprintItem thumbprint, string issuerAddress)
        {
            
            ConfigurationManagerHelper.SetValueOnKey(GetKey(issuerAddress), thumbprint.Thumbprint);
        }

        private static string GetKey(string issuerAddress)
        {
            var uri = new Uri(issuerAddress);
            return StardustInternalThumbprint+"."+uri.Host;
        }

        public ThumbprintItem GetChacedThumbprint(string issuerAddress)
        {
            var thumbprint= ConfigurationManagerHelper.GetValueOnKey(GetKey(issuerAddress));
            return thumbprint.IsNullOrWhiteSpace()
                ? new ThumbprintItem {Resolved = false}
                : new ThumbprintItem {Resolved = true, Expiry = DateTime.UtcNow.AddDays(1), Thumbprint = thumbprint};
        }
    }
}