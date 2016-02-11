using Stardust.Core.Security;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public static class KeyHelper
    {
        public static EncryptionKeyContainer SharedSecret
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
            key = "mayTheKeysSupportAllMyValues";
            ConfigurationManagerHelper.SetValueOnKey("stardust.ConfigKey", key, true);
            return key;
        }
    }

}