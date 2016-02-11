using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Stardust.Particles;
using Stardust.Stellar.ConfigurationReader;

namespace Stardust.Stellar
{
    internal class ConfigurationExtension : DynamicObject, IConfigurationExtensions
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object val = null;
            if (binder.Name.StartsWith("Secure"))
            {
                result = Secure(binder.Name.Replace("Secure", ""));
            }
            else if (binder.Name.StartsWith("Host"))
            {
                IConfigurationExtensionsCore value;
                val = Service[binder.Name.Replace("Service", "")];
            }
            else
            {
                val = this[binder.Name];
            }
            if (val == null) return base.TryGetMember(binder, out result);
            result = val;
            return true;
        }

        private readonly IConfigurationReader reader;


        private EnvironmentConfig environment;

        private ConfigurationSet set;

        private ServiceConfig service;

        public ConfigurationExtension(IConfigurationReader reader)
        {
            this.reader = reader;
            set = reader.GetConfiguration(CurrentSet, CurrentEnvironment);
            environment = set.Environments.SingleOrDefault(e => string.Equals(e.EnvironmentName, CurrentEnvironment, StringComparison.OrdinalIgnoreCase));
            var serviceCollection = new ServiceCollection(new Dictionary<string, IConfigurationExtensionsCore>());
            foreach (var serviceConfig in set.Services)
            {
                serviceCollection.Add(serviceConfig.ServiceName, new ConfigurationExtension(reader, serviceConfig));
            }
            Service = serviceCollection;
        }

        private static string CurrentSet
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("configSet");
            }
        }

        private ConfigurationExtension(IConfigurationReader reader, ServiceConfig service)
        {
            this.reader = reader;
            this.service = service;
        }

        public string this[string name]
        {
            get
            {
                if (environment != null)
                {
                    return environment.GetConfigParameter(name);
                }
                if (service != null)
                {
                    service.GetConfigParameter(name);
                }
                return null;
            }
        }

        public string Secure(string name)
        {
            if (environment != null)
            {
                return environment.GetSecureConfigParameter(name);
            }
            if (service != null)
            {
                service.GetSecureConfigParameter(name);
            }
            return null;
        }

        private static string CurrentEnvironment
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("environment");
            }
        }

        public IServiceCollection Service { get; private set; }
    }
}