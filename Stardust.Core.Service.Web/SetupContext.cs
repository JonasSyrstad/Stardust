using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using Stardust.Core.Security;
using Stardust.Core.Service.Web.Identity.Active;
using Stardust.Core.Wcf;
using Stardust.Interstellar;
using Stardust.Wormhole;
using Stardust.Nucleus;

namespace Stardust.Core.Service.Web
{

    public static class OAuthExtensions
    {
        /// <summary>
        /// Adds OAuth bearer token to wcf rest client requests.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ISetupContext MakeWcfOAuthAware(this ISetupContext context)
        {
            ((IClaimsSetupContext)context).MakeOAuthAwareService();
            return context;
        }
    }
    internal class SetupContext : IClaimsSetupContext, ISetupContext
    {
        private readonly HttpApplication Application;
        private bool PassiveFederationInitialized;

        internal SetupContext(HttpApplication application)
        {
            Application = application;
        }

        IClaimsSetupContext IClaimsSetupContext.LoadBindings<T>()
        {
            Resolver.LoadModuleConfiguration<T>();
            return this;
        }

        public IClaimsSetupContext MakeOAuthAwareService()
        {
            ServiceProxyBuilder.AddClientBehaviorExtentions(
                factory =>
                    {
                        if(factory.Endpoint.Behaviors.Find<AdalWcfClientInspector>()==null)
                            factory.Endpoint.Behaviors.Add(new AdalWcfClientInspector());
                    });
            return this;
        }

        public IClaimsSetupContext MakeOAuthAwareWebsite()
        {
            //How to do this here
            throw new NotImplementedException();
        }

        IClaimsSetupContext IClaimsSetupContext.LoadMapDefinitions<T>()
        {
            MapFactory.RegisterMappingDefinitions<T>();
            return this;
        }

        ISetupContext ISetupContext.LoadBindings<T>()
        {
            Resolver.LoadModuleConfiguration<T>();
            return this;
        }

        ISetupContext ISetupContext.LoadMapDefinitions<T>()
        {
            MapFactory.RegisterMappingDefinitions<T>();
            return this;
        }

        public IClaimsSetupContext MakeClaimsAware<T>(SessionSecurityTokenHandler tokenHandler = null) where T : ClaimsAuthenticationManager
        {
            if (PassiveFederationInitialized) throw new InvalidOperationException("Application is already made claims aware");
            Application.ConfigurePassiveFederation<T>(tokenHandler);
            PassiveFederationInitialized = true;
            return this;
        }

        public IClaimsSetupContext MakeClaimsAware(SessionSecurityTokenHandler tokenHandler = null)
        {
            if (PassiveFederationInitialized) throw new InvalidOperationException("Application is already made claims aware");
            Application.ConfigurePassiveFederation(tokenHandler);
            PassiveFederationInitialized = true;
            return this;
        }

        public IClaimsSetupContext SecureWcfRestServices()
        {
            Application.AuthenticateRequest += WcfRestClaimsModule.context_AuthenticateRequest;
            Application.EndRequest += WcfRestClaimsModule.context_EndRequest;
            return this;
        }

        public IClaimsSetupContext MakeClaimsAware<TAuth, TCache>(SessionSecurityTokenHandler tokenHandler = null)
            where TAuth : ClaimsAuthenticationManager
            where TCache : SessionSecurityTokenCache
        {
            if (PassiveFederationInitialized) throw new InvalidOperationException("Application is already made claims aware");
            Application.ConfigurePassiveFederation<TAuth, TCache>(tokenHandler);
            PassiveFederationInitialized = true;
            return this;
        }
        public IClaimsSetupContext MinifyCommonCookieNames()
        {
            AntiForgeryConfig.CookieName = "a";
            if (PassiveFederationInitialized)
                FederatedAuthentication.FederationConfiguration.CookieHandler.Name = "f";
            return this;
        }

        public IClaimsSetupContext SetAntiForgeryToken(string claimType)
        {
            if (!PassiveFederationInitialized) return this;
            AntiForgeryConfig.UniqueClaimTypeIdentifier = claimType;
            return this;
        }
    }
}