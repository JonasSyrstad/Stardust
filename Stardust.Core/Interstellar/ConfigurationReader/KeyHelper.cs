using Stardust.Core.Security;
using Stardust.Particles;

namespace Stardust.Interstellar.ConfigurationReader
{
    internal static class KeyHelper
    {
        internal static EncryptionKeyContainer SharedSecret
        {
            get
            {
                return new EncryptionKeyContainer(GetKeyFromConfig());
            }
        }

        private static string GetKeyFromConfig()
        {
            var key = ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigKey");
            if (key.ContainsCharacters()) return key;
            key = "defaultEncryptionKey";
            ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", key, true);
            return key;
        }
    }
}