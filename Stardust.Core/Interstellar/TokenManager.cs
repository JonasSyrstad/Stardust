//
// TokenManager.cs
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
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Stardust.Core.Security;
using Stardust.Interstellar.Endpoints;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public interface ITokenBridge
    {
        SecurityToken GetToken(string username, string password, BootstrapContext context = null);
    }

    public class TokenManager
    {
        private readonly NetworkCredential Credential;
        private static bool HaveCheckedTokenManager;
        private readonly string UserName;
        private readonly string Password;
        private readonly SecurityToken BootstrapToken;
        private readonly ConfigurationReader.Endpoint ServiceSettings;
        private static readonly Dictionary<string, SecurityToken> TokenCache = new Dictionary<string, SecurityToken>();
        private readonly ITokenManagerCache AlternateManagerCache;

        public static void SetBootstrapToken(BootstrapContext bootstrapToken)
        {
            if (bootstrapToken.IsNull()) return;
            ContainerFactory.Current.Bind(typeof(BootstrapContext), bootstrapToken, Scope.Context);
        }

        public static BootstrapContext GetBootstrapToken()
        {
            return (BootstrapContext)ContainerFactory.Current.Resolve(typeof(BootstrapContext), Scope.Context);
        }

        public TokenManager()
        {
            if (HaveCheckedTokenManager)
                return;
            AlternateManagerCache = Resolver.Activate<ITokenManagerCache>();
            HaveCheckedTokenManager = true;
        }

        public TokenManager(string serviceName, string username, string password)
            : this()
        {
            UserName = username;
            Password = password;
            ServiceSettings = HostConfigurator.SecureSettingsForService(serviceName);
        }

        public TokenManager(string serviceName, NetworkCredential credential)
            : this()
        {
            this.Credential = credential;

            ServiceSettings = HostConfigurator.SecureSettingsForService(serviceName);
        }

        public TokenManager(string serviceName, SecurityToken bootstrapToken, string username, string password)
            : this()
        {
            UserName = username;
            Password = password;
            BootstrapToken = bootstrapToken;
            ServiceSettings = HostConfigurator.SecureSettingsForService(serviceName);
        }

        public SecurityToken GetToken(string serviceRootUrl)
        {
            var rst = CreateIssueNewRequest(serviceRootUrl);
            return GetToken(serviceRootUrl, rst);
        }

        private RequestSecurityToken CreateIssueNewRequest(string serviceRootUrl)
        {
            var rst = new RequestSecurityToken
                          {
                              RequestType = RequestTypes.Issue,
                              KeyType = KeyTypes.Bearer,
                              AppliesTo = new EndpointReference(string.Format(ServiceSettings.Address, serviceRootUrl))
                          };
            return rst;
        }

        private SecurityToken GetSecurityToken(IWSTrustChannelContract channel, RequestSecurityToken rst)
        {
            try
            {
                RequestSecurityTokenResponse rstr = null;
                var token = channel.Issue(rst, out rstr);
                return token;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
                Logging.DebugMessage(string.Format("Request: user={0}, stsAddress={1} thumbprint={2}", UserName, ServiceSettings.StsAddress, ThumbprintResolver.ResolveThumbprint(ServiceSettings.Thumbprint, ServiceSettings.IssuerAddress)));
                throw;
            }
        }

        private WSTrustChannel CreateChannel()
        {

            var stsBinding = CreateStsBinding();
            UpdateStsSettingsFromEnvironment();
            var trustChannelFactory = new WSTrustChannelFactory(stsBinding, ServiceSettings.StsAddress)
                                          {
                                              TrustVersion =
                                                  TrustVersion
                                                  .WSTrust13
                                          };

            IncreaseTimeout(trustChannelFactory);
            trustChannelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
            if (UserName.IsNullOrWhiteSpace())
                trustChannelFactory.Credentials.Windows.ClientCredential = Credential;
            else
            {
                trustChannelFactory.Credentials.UserName.UserName = UserName;
                trustChannelFactory.Credentials.UserName.Password = Password;
            }
            trustChannelFactory.Endpoint.Behaviors.Add(new MustUnderstandBehavior(false));
            var channel = (WSTrustChannel)trustChannelFactory.CreateChannel();
            return channel;
        }

        private static void IncreaseTimeout(WSTrustChannelFactory trustChannelFactory)
        {
            trustChannelFactory.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            trustChannelFactory.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(10);
            trustChannelFactory.Endpoint.Binding.OpenTimeout = TimeSpan.FromMinutes(10);
            trustChannelFactory.Endpoint.Binding.CloseTimeout = TimeSpan.FromMinutes(10);
        }

        private void UpdateStsSettingsFromEnvironment()
        {
            var stsAdr =
                RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("StsAddress");
            if (stsAdr.ContainsCharacters())
                ServiceSettings.StsAddress = stsAdr;
            var issuerAdress = RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("IssuerAddress");
            if (stsAdr.ContainsCharacters())
                ServiceSettings.IssuerAddress = issuerAdress;
        }

        private Binding CreateStsBinding()
        {
            return new StsBindingCreator().SetClientMode(true).Create(ServiceSettings);
        }

        public SecurityToken GetTokenOnBehalfOf(string serviceRootUrl)
        {
            var rst = CreateDelegateTokenRequest(serviceRootUrl);
            return GetToken(serviceRootUrl, rst);
        }

        private SecurityToken GetToken(string serviceRootUrl, RequestSecurityToken rst)
        {
            var tokenKey = GetTokenKey(serviceRootUrl, rst);
            SecurityToken cachedToken;
            if (TryGetToken(tokenKey, out cachedToken)) return cachedToken;
            var channel = CreateChannel();
            var token = GetSecurityToken(channel, rst);
            AddTokenToCache(tokenKey, token);
            return token;
        }

        public void AddTokenToCache(string tokenKey, SecurityToken token)
        {
            if (AlternateManagerCache.IsInstance())
            {
                AlternateManagerCache.AddTokenToCache(tokenKey, token);
                return;
            }
            lock (TokenCache)
            {
                if (!TokenCache.ContainsKey(tokenKey))
                    TokenCache.Add(tokenKey, token);
            }
        }

        private bool TryGetToken(string tokenKey, out SecurityToken cachedToken)
        {
            if (AlternateManagerCache.IsInstance()) return AlternateManagerCache.TryGetToken(tokenKey, out cachedToken);
            SecurityToken value;
            if (TokenCache.TryGetValue(tokenKey, out value))
            {
                var cToken = value;
                if (cToken.ValidTo > DateTime.UtcNow)
                {
                    cachedToken = cToken;
                    return true;
                }
                lock (TokenCache)
                {
                    TokenCache.Remove(tokenKey);
                }
            }
            cachedToken = null;
            return false;
        }

        private string GetTokenKey(string serviceRootUrl, RequestSecurityToken rst)
        {
            if (rst.ActAs.IsInstance())
                return serviceRootUrl + rst.ActAs.GetSecurityToken().Id;
            return serviceRootUrl + UserName;
        }

        private RequestSecurityToken CreateDelegateTokenRequest(string serviceRootUrl)
        {
            var rst = new RequestSecurityToken
                          {
                              RequestType = RequestTypes.Issue,
                              KeyType = KeyTypes.Bearer,
                              AppliesTo = new EndpointReference(string.Format(ServiceSettings.Address, serviceRootUrl)),
                              ActAs = new SecurityTokenElement(BootstrapToken)
                          };
            return rst;
        }

        public static void InvalidateTokenCache()
        {
            if (HaveCheckedTokenManager)
            {
                var alternateManager = Resolver.Activate<ITokenManagerCache>();
                if (alternateManager.IsInstance())
                {
                    alternateManager.InvalidateTokenCache();
                    return;
                }
            }
            var list = (from t in TokenCache where t.Value.ValidTo < DateTime.UtcNow select t.Key).ToList();
            lock (TokenCache)
            {
                foreach (var item in list)
                    TokenCache.Remove(item);
            }
        }
    }
}