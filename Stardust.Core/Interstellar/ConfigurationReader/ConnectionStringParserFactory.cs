using Stardust.Particles;

namespace Stardust.Interstellar.ConfigurationReader
{
    public class ConnectionString
    {
        internal ConnectionString()
        {
            
        }

        public string ConfigSetName
        {
            get
            {
                var value = ConfigurationManagerHelper.GetValueOnKey("stardust.configSet");
                if (value.IsNullOrWhiteSpace())
                {
                    return ConfigurationManagerHelper.GetValueOnKey("configSet");
                }
                else
                {
                    return value;
                }
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.configSet", value, ConnectionStringParser.Forced);
                ConfigurationManagerHelper.SetValueOnKey("configSet", value, ConnectionStringParser.Forced);
            }
        }

        public bool UseAccessToken
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.useAccessToken", true); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.useAccessToken", value.ToString().ToLower(), ConnectionStringParser.Forced);
            }
        }

        public string ApiKey
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.accessToken"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.accessToken", value, ConnectionStringParser.Forced);
            }
        }

        public string TokenKey
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.accessTokenKey"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.accessTokenKey", value, ConnectionStringParser.Forced);
            }
        }

        public string Url
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.configLocation"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.configLocation", value, ConnectionStringParser.Forced);
            }
        }

        public string UserName
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.configUser"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.configUser", value, ConnectionStringParser.Forced);
            }
        }

        public string Password
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.configPassword"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.configPassword", value, ConnectionStringParser.Forced);
            }
        }

        public string Domain
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.configDomain"); }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.configDomain", value, ConnectionStringParser.Forced);
            }
        }

        public string Environment
        {
            get
            {
                var value = ConfigurationManagerHelper.GetValueOnKey("stardust.environment");
                if (value.IsNullOrWhiteSpace())
                {
                    return ConfigurationManagerHelper.GetValueOnKey("environment");
                }
                else
                {
                    return value;
                }
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.environment", value, ConnectionStringParser.Forced);
                ConfigurationManagerHelper.SetValueOnKey("environment", value, ConnectionStringParser.Forced);
            }
        }
    }
}