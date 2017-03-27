using Stardust.Core.Security;
using Stardust.Particles;

namespace Stardust.Interstellar.ConfigurationReader
{
    public class ConnectionStringParser
    {

        public static string CreateConnectionString(string configLocation, string applicationName, string environment, string tokenKey, string apiKey)
        {
            var raw = (string.Join(";", "stardust.useAccessToken=true;stardust.configLocation=" + configLocation, "stardust.configSet=" + applicationName, "stardust.environment=" + environment, "stardust.accessTokenKey=" + tokenKey, "stardust.accessToken=" + apiKey));
            return MakeConnectionString(raw);
        }

        private static string MakeConnectionString(string raw)
        {
            if (Encrypted)
                return raw.Encrypt(new EncryptionKeyContainer(ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigKey", "this is just bullshit")));
            return raw;
        }

        public static string CreateConnectionString(string configLocation, string applicationName, string environment, string username, string password, string domain)
        {
            var raw = (string.Join(";", "stardust.useAccessToken=false;stardust.configLocation=" + configLocation, "stardust.configSet=" + applicationName, "stardust.environment=" + environment, "stardust.configUser=" + username, "stardust.configPassword=" + password, "stardust.configDomain=" + domain));
            return MakeConnectionString(raw);
        }

        public static bool Encrypted { get; set; }
        public static bool Forced { get; set; }

        /// <summary>
        /// Parses the connection string from app settings.
        /// </summary>
        public static ConnectionString ParseConnectionString()
        {
            var items = GetConnectionString(ConfigurationManagerHelper.GetValueOnKey("stardust.configConnectionString")).Split(';');
            foreach (var item in items)
            {
                var keyValue = item.Split('=');
                ConfigurationManagerHelper.SetValueOnKey(keyValue[0], keyValue[1]);
            }
            return new ConnectionString();
        }



        public static ConnectionString ParseConnectionString(string connectionString)
        {
            var items = GetConnectionString(connectionString).Split(';');
            foreach (var item in items)
            {
                var keyValue = item.Split('=');
                ConfigurationManagerHelper.SetValueOnKey(keyValue[0], keyValue[1]);
            }
            return new ConnectionString();
        }

        private static string GetConnectionString(string connectionString)
        {
            var raw = connectionString;
            if (Encrypted)
                return raw.Decrypt(new EncryptionKeyContainer(ConfigurationManagerHelper.GetValueOnKey("stardust.ConfigKey", "this is just bullshit")));
            return raw;
        }
    }
}