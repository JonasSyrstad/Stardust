using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public static class ServiceDescriptionExtentions
    {
        /// <summary>
        /// Defaults for service configuration
        /// </summary>
        private static readonly Dictionary<string, string> WcfServiceParameters = new Dictionary<string, string>
        {
            {"MessageFormat","Mtom"},
            {"MaxMessageSize", "2147483647"},
            {"HostNameComparisonMode", "StrongWildcard"},
            {"TextEncoding", "UTF8"},
            {"Address", "https://{0}/"},
            {"Durable", "true"},
            {"MaxReceivedSize", "2147483647"},
            {"MaxBufferPoolSize", "2147483647"},
            {"MaxBufferSize", "2147483647"},
            {"TransferMode","Buffered"},
            {"CloseTimeout", "100"},
            {"OpenTimeout", "100"},
            {"ReceiveTimeout", "100"},
            {"SendTimeout", "100"},
            {"IssuerName", "{0}"},
            {"IssuerActAsAddress","https://{0}/adfs/services/trust/13/usernamemixed"},
            {"IssuerMetadataAddress", "https://{0}/FederationMetadata/2007-06/FederationMetadata.xml"},
            {"Audience", "https://{0}/"},
            {"Realm", "https://{0}/"},
            {"Thumbprint", "{0}"},
            {"CertificateValidationMode","None"}, 
            {"OverrideSslSecurity","true"},
            {"IssuerAddress", "https://{0}"},
            {"StsAddress", "https://{0}/adfs/services/trust/13/usernamemixed"},
            {"Ignore","false"},
            {"EnforceCertificateValidation","false"},
            {"RequireHttps","false"}
        };

        private static readonly Dictionary<string, string> CustomServiceParameters = new Dictionary<string, string>
        {
            {"Address", "https://{0}/"},
            {"Key","{0}"},
            {"UserName","{0}"},
            {"ContainerName","{0}"},
            {"SenderName","{0}"},
            {"Port","{0}"},
            {"KeepAlive","{0}"},
            {"Metadata","{0}"}
        };


        internal static bool ParameterIsEnvironmental(string name, string enpointName)
        {
            return String.Equals(enpointName, "custom", StringComparison.OrdinalIgnoreCase) || CreateEnvironmentExtentions.ServiceParameters.Contains(name);
        }

        internal static bool ParameterIsEnvironmentalPrService(string name)
        {
            return CreateEnvironmentExtentions.PrServiceParameters.Contains(name);
        }

        public static IServiceDescription CreateChild(this IServiceDescription service, ConfigurationContext context, ref IConfigSet configSet)
        {
            var child = context.ServiceDescriptions.Create();
            child.Name = service.Name;
            child.ConfigSetNameId = configSet.Id;
            child.ConfigSet = configSet;
            child.PatentServiceDescription = service;
            service.ChildServiceDescriptions.Add(child);
            context.SaveChanges();
            foreach (var endpoint in service.Endpoints)
            {
                var newEndpoint = endpoint.CreateChild(context, ref child);
                child.Endpoints.Add(newEndpoint);

            }
            foreach (var environment in configSet.Environments)
            {
                var addressOverride = string.Format("{0}_Address", service.Name);
                var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => String.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                    environment.CreateSubstitutionParameters(context, addressOverride);
            }
            return child;
        }

        public static IServiceHostSettings CreateChild(this IServiceHostSettings service, ConfigurationContext context, ref IConfigSet configSet)
        {
            var child = context.ServiceHostSettingss.Create();
            child.Name = service.Name;
            child.ConfigSetNameId = configSet.Id;
            child.ConfigSet = configSet;
            child.Parent = service;
            foreach (var parameter in service.Parameters)
            {
                parameter.CreateChild(context, child);
            }
            return child;
        }

        public static IServiceDescription CreateService(this IConfigSet configSet, ConfigurationContext context, string serviceName, List<string> parameters = null)
        {
            var service = context.ServiceDescriptions.Create();
            service.Name = serviceName;
            service.ConfigSetNameId = configSet.Id;
            service.ConfigSet = configSet;
            foreach (var environment in configSet.Environments)
            {
                var addressOverride = string.Format("{0}_Address", serviceName);
                var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => String.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                    environment.CreateSubstitutionParameters(context, addressOverride);
            }
            AddToChildren(configSet, context, service);
            return service;
        }

        private static void AddToChildren(IConfigSet configSet, ConfigurationContext context, IServiceDescription service)
        {
            foreach (var childConfigSet in configSet.ChildConfigSets)
            {
                var c = childConfigSet;
                var child = service.CreateChild(context, ref c);
                c.Services.Add(child);
                AddToChildren(c, context, child);
            }
        }

        public static IServiceHostParameter CreateParameter(this IServiceHostSettings serviceHost, ConfigurationContext context, string name, bool isSecureString,bool isEnvironmental)
        {
            var serviceHostParameter = context.ServiceHostParameters.Create();
            serviceHostParameter.ServiceHostSettingsNameId = serviceHost.Id;
            serviceHostParameter.ServiceHost = serviceHost;
            serviceHostParameter.Name = name;
            serviceHostParameter.IsSecureString = isSecureString;
            serviceHostParameter.IsEnvironmental = isEnvironmental;
            serviceHost.Parameters.Add(serviceHostParameter);
            AddToChildren(serviceHost, context, serviceHostParameter);
            return serviceHostParameter;
        }

        private static void AddToChildren(IServiceHostSettings serviceHost, ConfigurationContext context, IServiceHostParameter substitutionParameter)
        {
            foreach (var serviceHostSettingse in serviceHost.Children)
            {
                var child = substitutionParameter.CreateChild(context, serviceHostSettingse);
                AddToChildren(serviceHostSettingse, context, child);
            }
        }

        private static IServiceHostParameter CreateChild(this IServiceHostParameter endpointParameter, ConfigurationContext context, IServiceHostSettings serviceHost)
        {
            var child = context.ServiceHostParameters.Create();
            child.Name = endpointParameter.Name;
            child.ServiceHostSettingsNameId = serviceHost.Id;
            child.ServiceHost = serviceHost;
            child.Parent = endpointParameter;
            child.IsSecureString = endpointParameter.IsSecureString;
            serviceHost.Parameters.Add(child);
            return child;
        }

        public static IServiceHostSettings CreateServiceHost(this IConfigSet configSet, ConfigurationContext context, string serviceName)
        {
            var service = context.ServiceHostSettingss.Create();
            service.Name = serviceName;
            service.ConfigSetNameId = configSet.Id;
            service.ConfigSet = configSet;
            foreach (var environment in configSet.Environments)
            {
                var addressOverride = string.Format("{0}_Address", serviceName);
                var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                    environment.CreateSubstitutionParameters(context, addressOverride);
            }
            AddToChildren(configSet, context, service);
            return service;
        }

        private static void AddToChildren(IConfigSet configSet, ConfigurationContext context, IServiceHostSettings service)
        {
            foreach (var childConfigSet in configSet.ChildConfigSets)
            {
                var c = childConfigSet;
                var childService = service.CreateChild(context, ref c);
                AddToChildren(c, context, childService);
            }
        }

        public static IEndpoint CreateEndpoint(this IServiceDescription service, ConfigurationContext context, string endpointName, bool ignoreParameters = false, List<string> parameters = null)
        {
            var endpoint = context.Endpoints.Create();
            endpoint.Name = endpointName;
            endpoint.ServiceNameId = service.Id;
            if (service.Endpoints.Count == 0)
                service.ClientEndpointValue = endpoint.Name;
            if (!ignoreParameters)
            {
                var cParam = CustomServiceParameters;
                if (parameters.IsInstance())
                    cParam = parameters.ToDictionary(s => s, v => "{0}");
                AddServiceParameters(service, context, endpoint, string.Equals(endpointName, "custom", StringComparison.OrdinalIgnoreCase) ? cParam : WcfServiceParameters);
            }
            service.Endpoints.Add(endpoint);
            context.SaveChanges();
            CreateSubstitutionParameters(service, context, endpointName, endpoint);
            return endpoint;
        }

        private static void CreateSubstitutionParameters(IServiceDescription service, ConfigurationContext context,
            string endpointName, IEndpoint endpoint)
        {
            if (endpointName == "custom")
            {
                foreach (var environment in service.ConfigSet.Environments)
                {
                    foreach (var param in endpoint.Parameters)
                    {
                        var addressOverride = string.Format("{0}_{1}", service.Name, param.Name);
                        var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                        if (overrideProp.IsNull())
                            environment.CreateSubstitutionParameters(context, addressOverride);
                    }
                }
            }
        }

        public static void AddServiceParameters(IServiceDescription service, ConfigurationContext context, IEndpoint endpoint, Dictionary<string, string> wcfServiceParameters)
        {
            foreach (var serviceParameter in wcfServiceParameters.Keys)
            {
                var parameter = context.EndpointParameters.Create();
                parameter.Name = serviceParameter;
                parameter.EndpointNameId = endpoint.Id;
                parameter.Endpoint = endpoint;
                parameter.ConfigurableForEachEnvironment = ParameterIsEnvironmental(serviceParameter, endpoint.Name);
                parameter.IsPerService = ParameterIsEnvironmentalPrService(serviceParameter);
                if (!string.Equals(endpoint.Name, "custom", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.ItemValue = serviceParameter == "Address" ? string.Format("{0}{1}.svc/{2}", wcfServiceParameters[serviceParameter], service.Name, endpoint.Name) : wcfServiceParameters[serviceParameter];
                }
                else
                {
                    parameter.ItemValue = "{0}";
                }
                endpoint.Parameters.Add(parameter);
            }
        }

        public static void AddParameter(this IEndpoint endpoint, ConfigurationContext context, string parameterName, string parameterValue, bool isSubstitutionParameter)
        {
            var parameter = context.EndpointParameters.Create();
            parameter.Name = parameterName;
            parameter.EndpointNameId = endpoint.Id;
            parameter.Endpoint = endpoint;
            parameter.ConfigurableForEachEnvironment = isSubstitutionParameter;
            parameter.IsPerService = isSubstitutionParameter;
            parameter.ItemValue = "{0}";
            endpoint.Parameters.Add(parameter);
            AddToChildren(endpoint.ServiceDescription, context, endpoint);
            if (!isSubstitutionParameter) return;
            foreach (var environment in endpoint.ServiceDescription.ConfigSet.Environments)
            {
                var addressOverride = string.Format("{0}_{1}", endpoint.ServiceDescription.Name, parameterName);
                var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => String.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                    environment.CreateSubstitutionParameters(context, addressOverride);
            }
        }

        private static void AddToChildren(IServiceDescription service, ConfigurationContext context, IEndpoint endpoint)
        {
            foreach (var child in service.ChildServiceDescriptions)
            {
                var c = child;
                var childEndpoint = endpoint.CreateChild(context, ref c);
                child.Endpoints.Add(childEndpoint);
                context.SaveChanges();
                AddToChildren(c, context, childEndpoint);
            }
        }

        public static IEndpoint CreateChild(this IEndpoint endpoint, ConfigurationContext context, ref IServiceDescription service)
        {
            var child = context.Endpoints.Create();
            child.Name = endpoint.Name;
            child.ServiceNameId = service.Id;
            child.ServiceDescription = service;
            context.SaveChanges();
            foreach (var endpointParameter in endpoint.Parameters)
            {
                var parameter = CreateChild(endpointParameter, context, child);
                child.Parameters.Add(parameter);
            }
            return child;
        }

        private static IEndpointParameter CreateChild(IEndpointParameter endpointParameter, ConfigurationContext context, IEndpoint enpoint)
        {
            var child = context.EndpointParameters.Create();
            child.Name = endpointParameter.Name;
            child.EndpointNameId = enpoint.Id;
            child.Parent = endpointParameter;
            child.Endpoint = enpoint;
            child.IsPerService = endpointParameter.IsPerService;
            child.ConfigurableForEachEnvironment = endpointParameter.ConfigurableForEachEnvironment;
            return child;
        }
    }
}