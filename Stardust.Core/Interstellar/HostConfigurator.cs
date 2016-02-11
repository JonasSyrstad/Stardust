//
// HostConfigurator.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using Stardust.Core.Security;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Configures the WCF service to be claims aware. 
    /// </summary>
    public static class HostConfigurator
    {
        /// <summary>
        /// Call this method to set up the service as claims aware.
        /// </summary>
        /// <param name="config"></param>
        public static void Configure(ServiceConfiguration config)
        {
            var secureSettings = config.GetSecureSettingsForService();
            var serviceRootUrl = config.GetServiceRootUrl();
            if (secureSettings.IsNull()) return;
            TurnOfSslCertificateValidation(secureSettings);
            config.IdentityConfiguration = new IdentityConfiguration()
            {
                TrustedStoreLocation = new StoreLocation(),
                AudienceRestriction = { AudienceMode = AudienceUriMode.Always },
                SaveBootstrapContext = true,
                IssuerTokenResolver = new IssuerTokenResolver(),
                CertificateValidationMode = secureSettings.CertificateValidationMode.ParseAsEnum(X509CertificateValidationMode.PeerTrust),
            };
            config.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(string.Format(secureSettings.Audience, serviceRootUrl)));
            config.IdentityConfiguration.IssuerNameRegistry = CreateIssuerNameRegistry(secureSettings);
            config.UseIdentityConfiguration = true;
            ThumbprintResolver.RegisterServiceHost(config);

        }

        private static string GetServiceRootUrl(this ServiceConfiguration config)
        {
            var key = config.GetEnvironmentKey();
            var value = RuntimeFactory.CreateRuntime(Scope.PerRequest).Context.GetEnvironmentConfiguration().GetConfigParameter(key);
            if (value.ContainsCharacters()) return value;
            return RuntimeFactory.CreateRuntime(Scope.PerRequest).Context.GetEnvironmentConfiguration().GetConfigParameter(key + "_Address");
        }

        private static string GetEnvironmentKey(this ServiceConfiguration config)
        {
            var attribute = config.Description.ServiceType.GetServiceNameAttribute();
            return attribute == null ? config.Description.ServiceType.Name : attribute.UrlRootName;
        }

        private static void TurnOfSslCertificateValidation(Endpoint secureSettings)
        {
            if (secureSettings.OverrideSslSecurity)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);
            }
        }

        private static Endpoint GetSecureSettingsForService(this ServiceConfiguration config)
        {
            var serviceName = GetServiceName(config.Description.ServiceType);
            return SecureSettingsForService(serviceName);
        }

        internal static Endpoint SecureSettingsForService(string serviceName)
        {
            var settings = RuntimeFactory.CreateRuntime().Context.GetEndpointConfiguration(serviceName);
            var secureSettings = (settings.GetEndpoint(settings.ActiveEndpoint));
            return secureSettings;
        }

        private static ConfigurationBasedIssuerNameRegistry CreateIssuerNameRegistry(Endpoint serviceInterface)
        {
            GetStsSettingsFromEnvironment(serviceInterface);
            var registry = new ConfigurationBasedIssuerNameRegistry();
            registry.AddTrustedIssuer(ThumbprintResolver.ResolveThumbprint(serviceInterface.Thumbprint, serviceInterface.IssuerAddress), serviceInterface.IssuerAddress);
            return registry;
        }

        private static void GetStsSettingsFromEnvironment(Endpoint serviceInterface)
        {
            var thumbprint =
                RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("Thumbprint");
            if (thumbprint.ContainsCharacters())
                serviceInterface.Thumbprint = thumbprint;
            var issuerName =
                RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("IssuerName");
            if (issuerName.ContainsCharacters())
                serviceInterface.IssuerName = issuerName;
        }

        internal static string GetServiceName(this Type serviceType)
        {
            var attribute = GetServiceNameAttribute(serviceType);
            return attribute == null ? serviceType.Name : attribute.ServiceName;
        }

        private static ServiceNameAttribute GetServiceNameAttribute(this Type serviceType)
        {
            return GetServiceContract(serviceType).GetAttribute<ServiceNameAttribute>();
        }

        private static Type GetServiceContract(Type serviceType)
        {
            return (from i in serviceType.GetInterfaces() where i != typeof(IServiceBase) select i).First();
        }
    }
}
