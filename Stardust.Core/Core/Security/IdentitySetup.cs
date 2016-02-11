//
// IdentitySetup.cs
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
using System.IdentityModel.Services;
using System.IdentityModel.Services.Tokens;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Web;
using Stardust.Core.Wcf;
using Utilities = Stardust.Interstellar.Utilities.Utilities;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;
using Stardust.Nucleus.Extensions;
using Stardust.Particles;
using Stardust.Interstellar;

namespace Stardust.Core.Security
{
    public static class IdentitySetup
    {
        private static readonly object Triowing = new object();
        private static Type AuthorizationManager;
        private static Type CahceModule;
        public static void SetClaimsManager<T>() where T : ClaimsAuthenticationManager
        {
            lock (Triowing)
            {
                AuthorizationManager = typeof(T);
            }
        }

        public static void ConfigurePassiveFederation<TAuth, TCache>(this HttpApplication application, SessionSecurityTokenHandler tokenHandler)
            where TAuth : ClaimsAuthenticationManager
            where TCache : SessionSecurityTokenCache
        {
            if (WebServerConfiguration.IsConfiguredAsWebFront)
                Logging.DebugMessage("Stardust web requirements configured");
            lock (Triowing)
            {
                AuthorizationManager = typeof(TAuth);
                CahceModule = typeof(TCache);
            }
            Logging.DebugMessage("Managers added to initializer");
            application.ConfigurePassiveFederation(tokenHandler);
        }

        public static void ConfigurePassiveFederation<T>(this HttpApplication application, SessionSecurityTokenHandler tokenHandler) where T : ClaimsAuthenticationManager
        {
            Logging.DebugMessage("Initializing Stardust...");
            SetClaimsManager<T>();
            Logging.DebugMessage("Claims manager set");
            application.ConfigurePassiveFederation(tokenHandler);
        }

        public static void ConfigurePassiveFederation(this HttpApplication application, SessionSecurityTokenHandler tokenHandler)
        {
            using (ContainerFactory.Current.ExtendScope(Scope.Context))
            {
                Logging.DebugMessage("Initializing Runtime");
                var runtime = CreateRuntime();
                Logging.DebugMessage("Runtime initialized");
                var settings = GetSettings(runtime);
                Logging.DebugMessage("Settings obtained");
                var rootUrl = GetRootUrl(runtime);
                Logging.DebugMessage("Root url obtained");
                DisableChainValidation(settings);
                Logging.DebugMessage("Chain validation set");
                ConfigureIdentitySettings(settings, rootUrl, tokenHandler);
                Logging.DebugMessage("Identity settins configured");
                AddAudience(settings, rootUrl);
                Logging.DebugMessage("Audience added");
                if (!FederatedAuthentication.FederationConfiguration.IsInitialized)
                {
                    FederatedAuthentication.FederationConfiguration.Initialize();
                    Logging.DebugMessage("Federated authentication initialized");
                }
                Logging.DebugMessage("Configuration completed");
            }
        }

        static void MvcApplication_BeginRequest(object sender, EventArgs e)
        {
            FederatedAuthentication.WSFederationAuthenticationModule.SessionSecurityTokenCreated -= WSFederationAuthenticationModule_SessionSecurityTokenCreated;
            FederatedAuthentication.WSFederationAuthenticationModule.SessionSecurityTokenCreated += WSFederationAuthenticationModule_SessionSecurityTokenCreated;
        }


        public static void WSFederationAuthenticationModule_SessionSecurityTokenCreated(object sender, SessionSecurityTokenCreatedEventArgs e)
        {
            e.SessionToken.IsReferenceMode = true;
        }

        private static string GetRootUrl(IRuntime runtime)
        {
            var rootUrl = runtime.Context.GetEnvironmentConfiguration().GetConfigParameter(Utilities.GetServiceName());
            return rootUrl;
        }

        private static IdentitySettings GetSettings(IRuntime runtime)
        {
            var settings = runtime.Context.GetServiceConfiguration().IdentitySettings;
            return settings;
        }

        private static IRuntime CreateRuntime()
        {
            var runtime = RuntimeFactory.CreateRuntime(Scope.PerRequest);
            runtime.NoTrace = true;
            Logging.DebugMessage("Runtime created");
            runtime.SetEnvironment(Utilities.GetEnvironment());
            Logging.DebugMessage("Environment set");
            var serviceName = Utilities.GetServiceName();
            Logging.DebugMessage("ServiceName obtained");
            runtime.SetServiceName(new object(), serviceName, "IdentitySetup");
            Logging.DebugMessage("Service name set");
            return runtime;
        }

        private static void DisableChainValidation(IdentitySettings settings)
        {
            if (!settings.EnforceCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback = OnServerCertificateValidationCallback;
            }
        }

        private static void AddAudience(IdentitySettings settings, string rootUrl)
        {
            foreach (var audience in settings.Audiences)
            {
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris
                    .Add(new Uri(string.Format(audience, rootUrl)));
            }
        }

        private static void ConfigureIdentitySettings(IdentitySettings settings, string rootUrl, SessionSecurityTokenHandler tokenHandler)
        {
            GetStsSettingsFromEnvironment(settings);
            var identitySettings = ConfigureWithExternalModules();
            SetIssuer(settings, identitySettings);
            SetCertificateValidationMode(settings, identitySettings);
            ChangeTokenHandler(tokenHandler, identitySettings);
            ConfigureFederationSettings(settings, rootUrl);
            if (FederatedAuthentication.SessionAuthenticationModule.IsInstance())
                FederatedAuthentication.SessionAuthenticationModule.IsReferenceMode = true;
            ThumbprintResolver.RegisterWeb(identitySettings);
        }

        private static void SetCertificateValidationMode(IdentitySettings settings, IdentityConfiguration identitySettings)
        {
            identitySettings.CertificateValidationMode =
                settings.CertificateValidationMode.ParseAsEnum(X509CertificateValidationMode.None);
            switch (identitySettings.CertificateValidationMode)
            {
                case X509CertificateValidationMode.None:
                    identitySettings.CertificateValidator = X509CertificateValidator.None;
                    break;
                case X509CertificateValidationMode.ChainTrust:
                    identitySettings.CertificateValidator = X509CertificateValidator.ChainTrust;
                    break;
                case X509CertificateValidationMode.PeerOrChainTrust:
                    identitySettings.CertificateValidator = X509CertificateValidator.PeerOrChainTrust;
                    break;
                case X509CertificateValidationMode.PeerTrust:
                    identitySettings.CertificateValidator = X509CertificateValidator.PeerTrust;
                    break;
            }
        }

        private static
            void ChangeTokenHandler(SessionSecurityTokenHandler tokenHandler, IdentityConfiguration identitySettings)
        {
            var itemTOremove =
                (from i in identitySettings.SecurityTokenHandlers
                 where i.GetType().Implements<SessionSecurityTokenHandler>() || i is SessionSecurityTokenHandler
                 select i).SingleOrDefault();
            if (itemTOremove.IsInstance())
                identitySettings.SecurityTokenHandlers.Remove(itemTOremove);
            if (tokenHandler.IsInstance())
                identitySettings.SecurityTokenHandlers.Add(tokenHandler);
            else
                identitySettings.SecurityTokenHandlers.Add(new MachineKeySessionSecurityTokenHandler());
            Logging.DebugMessage("Configured Identity token handler");
        }

        private static void ConfigureFederationSettings(IdentitySettings settings, string rootUrl)
        {
            var federationSettings = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration;
            federationSettings.PassiveRedirectEnabled = (ConfigurationManagerHelper.GetValueOnKey("stardust.AllowAnonymous") == "true" || ConfigurationManagerHelper.GetValueOnKey("stardust.AllowAnonymous").IsNullOrWhiteSpace());
            federationSettings.Realm = string.Format(settings.Realm, rootUrl);
            federationSettings.Issuer = settings.IssuerAddress;
            federationSettings.RequireHttps = settings.RequireHttps;
            FederatedAuthentication.FederationConfiguration.CookieHandler.RequireSsl = settings.RequireHttps;
            Logging.DebugMessage("Federation settings configured");
        }

        private static void SetIssuer(IdentitySettings settings, IdentityConfiguration identitySettings)
        {
            var issuers = new ConfigurationBasedIssuerNameRegistry();
            issuers.AddTrustedIssuer(ThumbprintResolver.ResolveThumbprint(settings.Thumbprint, settings.IssuerAddress), settings.IssuerAddress);
            Logging.DebugMessage("Issuer settings configured");
            identitySettings.IssuerNameRegistry = issuers;
            Logging.DebugMessage("Issuer name registry is added");
        }

        private static IdentityConfiguration ConfigureWithExternalModules()
        {
            var identitySettings = FederatedAuthentication.FederationConfiguration.IdentityConfiguration;
            identitySettings.SaveBootstrapContext = true;
            identitySettings.AudienceRestriction.AudienceMode = AudienceUriMode.Always;
            if (AuthorizationManager != null)
            {
                identitySettings.ClaimsAuthenticationManager = AuthorizationManager.Activate<ClaimsAuthenticationManager>();
                Logging.DebugMessage("AuthenticationManager obtained");
            }
            if (CahceModule.IsInstance())
            {
                identitySettings.Caches.SessionSecurityTokenCache = CahceModule.Activate<SessionSecurityTokenCache>();
                Logging.DebugMessage("SessionSecurityTokenCache obtained");
            }
            Logging.DebugMessage("Identity settings obtained. External components added.");
            return identitySettings;
        }

        private static void GetStsSettingsFromEnvironment(IdentitySettings settings)
        {
            var env = RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration();
            var thumbPrint = env.GetConfigParameter("Thumbprint");
            if (thumbPrint.ContainsCharacters())
                settings.Thumbprint = thumbPrint;
            var issuerName = env.GetConfigParameter("IssuerName");
            if (issuerName.ContainsCharacters())
                settings.IssuerName = issuerName;
            var addr = env.GetConfigParameter("IssuerAddress");
            if (addr.ContainsCharacters())
                settings.IssuerAddress = addr;
            Logging.DebugMessage("STS settings obtained");
        }

        private static bool OnServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
