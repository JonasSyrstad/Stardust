using System;
using System.Collections.Specialized;
using System.Configuration;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Stellar.ConfigurationReader;

namespace Stardust.Stellar
{
    public static class StardustConfigurationManager
    {
        private static IConfigurationReader reader;

        private static ConfigurationExtension current;

        public static IConfigurationReader GetConfigurationReader()
        {
            if (reader.IsNull()) reader = Resolver.Activate<IConfigurationReader>();
            return reader;
        }

        public static IConfigurationExtensions Dynamic(this Configuration manager)
        {
            return CreateConfigurationReader();
        }

        public static IConfigurationExtensions Dynamic(this NameValueCollection manager)
        {
            return CreateConfigurationReader();
        }

        public static IConfigurationExtensions Dynamic(this AppDomain manager)
        {
            return CreateConfigurationReader();
        }

        public static IConfigurationExtensions Current
        {
            get
            {
                return CreateConfigurationReader();
            }
        }

        private static IConfigurationExtensions CreateConfigurationReader()
        {
            if (current == null)
                current = new ConfigurationExtension(GetConfigurationReader());
            return Current;
        }
    }
}
