//
// ServiceContainer.cs
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
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Xml;
using Stardust.Core.Wcf;
using Stardust.Interstellar.Endpoints;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Wraps an service interface in a disposable containter.
    /// The container creates a proxy at runtime and initializes calimsbased auth if applicable.
    /// It also maintains instance id for messages implementing IRequestBase for logging and tracing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceContainer<T> : IServiceContainer<T>
    {
        public bool ReregisterForGC;
        private ChannelFactory<T> ClientFactory;
        private T Client;
        private string Url;
        private string ServiceName;
        private CallStackItem CallStack;
        private bool UseSecureChannel;

        private BootstrapContext BootstrapContext
        {
            get
            {
                return bootstrapContext;
            }
            set
            {

                bootstrapContext = value;
            }
        }

        private string UserName;
        private string Password;
        private string ServiceRootUrl;
        private NetworkCredential credential;

        private BootstrapContext bootstrapContext;

        public bool Initialized { get; private set; }

        public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
        {
            var message = executor();
            if (CallStack.IsInstance())
                CallStack.CallStack.Add(message.ResponseHeader.CallStack);
            return message;
        }

        public ServiceContainer<T> SetCallStack(CallStackItem callstack)
        {
            CallStack = callstack;
            return this;
        }

        public void InitializeContainer(ChannelFactory<T> clientFactory, string url, string serviceName)
        {
            if (clientFactory.Endpoint.Binding is WS2007FederationHttpBinding || (clientFactory.Endpoint.Binding is SecureWebHttpBinding && clientFactory.Endpoint.Behaviors.Find<RestTokenHandler>() != null))
            {
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    var envUser = RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetConfigParameter("ServiceAccountName");
                    var envPass = RuntimeFactory.CreateRuntime().Context.GetEnvironmentConfiguration().GetSecureConfigParameter("ServiceAccountPassword");
                    UserName = envUser.ContainsCharacters() ? envUser : RuntimeFactory.CreateRuntime().Context.GetServiceConfiguration().Username;
                    Password = envPass.ContainsCharacters() ? envPass : RuntimeFactory.CreateRuntime().Context.GetServiceConfiguration().Password;
                }
            }
            ClientFactory = clientFactory;
            AddBehaviors(clientFactory);
            AddInstanceIdentityInspector(clientFactory);
            Url = url;
            ServiceName = serviceName;
            Initialized = true;
            if (ReregisterForGC)
                GC.ReRegisterForFinalize(this);
        }

        private void AddBehaviors(ChannelFactory<T> clientFactory)
        {
            if (clientFactory.Endpoint.Binding is WebHttpBinding)
            {
                if (clientFactory.Endpoint.Behaviors.Find<WebHttpBehavior>() == null)
                    clientFactory.Endpoint.Behaviors.Add(new WebHttpBehavior() { HelpEnabled = true, FaultExceptionEnabled = true, DefaultOutgoingRequestFormat = WebMessageFormat.Json, DefaultOutgoingResponseFormat = WebMessageFormat.Json, AutomaticFormatSelectionEnabled = true, DefaultBodyStyle = WebMessageBodyStyle.Wrapped });
            }
            if (!(clientFactory.Endpoint.Binding is WS2007FederationHttpBinding)) return;
            if (TokenManager.GetBootstrapToken() == null)
                Initialize(true);
            else
                Initialize(TokenManager.GetBootstrapToken());
        }

        private static void AddInstanceIdentityInspector(ChannelFactory clientFactory)
        {
            try
            {
                var inspector = clientFactory.Endpoint.Behaviors.Find<InspectorInserter>();
                if (inspector == null)
                {
                    clientFactory.Endpoint.Behaviors.Add(new InspectorInserter());

                }
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, "AddInstanceIdentityInspector");
            }
            foreach (var operation in clientFactory.Endpoint.Contract.Operations)
            {
                try
                {
                    if (operation.Faults.All(fd => fd.DetailType != typeof(ErrorMessage)))
                        operation.AddFaultContract();
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, "AddInstanceIdentityInspector - fault contract");
                }
            }
        }

        /// <summary>
        /// Initailizes the container to use UserName/Password for authentication
        /// </summary>
        /// <param name="useSecure"></param>
        /// <returns></returns>
        public IServiceContainer<T> Initialize(bool useSecure = false)
        {
            UseSecureChannel = useSecure;
            return this;
        }

        /// <summary>
        /// Sets the bootstrap context used with Claims aware WCF services
        /// </summary>
        /// <param name="bootstrapContext"></param>
        /// <returns></returns>
        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            BootstrapContext = bootstrapContext;
            return this;
        }

        /// <summary>
        /// Sets the service credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            if (!username.IsNullOrWhiteSpace())
            {
                UserName = username;
                Password = password;
            }
            return this;
        }

        /// <summary>
        /// Creates and returns a WCF proxy object for the interface.
        /// </summary>
        /// <returns></returns>
        public T GetClient()
        {
            if (Client.IsNull())
            {
                var endpointAdress = Resolver.Activate<IUrlFormater>().FormatUrl(Url, ServiceName);
                CreateClient(endpointAdress);
                var channel = Client as IClientChannel;
                if (channel != null)
                {
                    channel.UnknownMessageReceived += channel_UnknownMessageReceived;
                    channel.Faulted += channel_Faulted;
                }
            }
            return Client;
        }

        private void CreateClient(EndpointAddress endpointAdress)
        {
            var rest = ClientFactory.Endpoint.Behaviors.Find<RestTokenHandler>();
            if (rest != null)
            {
                var tokenManager = FindTokenManager();
                RuntimeFactory.Current.GetStateStorageContainer().TryAddStorageItem(tokenManager);
                Client = ClientFactory.CreateChannel(endpointAdress);
            }
            else
            {
                if (ConfigurationManagerHelper.GetValueOnKey("stardust.RelayToken") == "true")
                {
                    if (BootstrapContext != null)
                    {
                        Client = ClientFactory.CreateChannelWithIssuedToken(BootstrapContext.SecurityToken);
                        return;
                    }
                }
                if (BootstrapContext.IsInstance()) Client = ClientFactory.CreateChannelWithIssuedToken(GetTokenOnBehalfOf(), endpointAdress);
                else if (UseSecureChannel) Client = ClientFactory.CreateChannelWithIssuedToken(GetToken(), endpointAdress);
                else Client = ClientFactory.CreateChannel(endpointAdress);
            }   
        }

        private TokenManager FindTokenManager()
        {
            return BootstrapContext.IsInstance() ? GetOnBehalfOfManager() : GetTokenManager();
        }

        void channel_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            var channel = Client as IClientChannel;
            if (channel != null)
            {
                SetConnectivityErrorFlag(new CommunicationObjectFaultedException("Unknown message received"));
            }
        }

        private void channel_Faulted(object sender, EventArgs e)
        {
            var channel = Client as IClientChannel;
            if (channel != null)
            {
                SetConnectivityErrorFlag(new CommunicationObjectFaultedException("Error connecting to service"));
                channel.Dispose();
            }
            try
            {
                GetClient();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void SetConnectivityErrorFlag(Exception ex)
        {
            RuntimeFactory.Current.GetStateStorageContainer().TryAddStorageItem(ex.Message, "connectionErrorMessage");
            RuntimeFactory.Current.GetStateStorageContainer().TryAddStorageItem(true, "connectionError");
        }

        private SecurityToken GetTokenOnBehalfOf()
        {
            var manager = GetOnBehalfOfManager();
            return manager.GetTokenOnBehalfOf(GetUrl());
        }

        private TokenManager GetOnBehalfOfManager()
        {
            var manager = new TokenManager(ServiceName, GetTokenFromBootstrap(), UserName, Password);
            return manager;
        }

        private SecurityToken GetTokenFromBootstrap()
        {
            if (BootstrapContext.SecurityToken != null)
                return BootstrapContext.SecurityToken;
            var handlers = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers;
            return handlers.ReadToken(new XmlTextReader(new StringReader(BootstrapContext.Token)));
        }

        private SecurityToken GetToken()
        {
            var manager = GetTokenManager();
            return manager.GetToken(GetUrl());
        }

        private TokenManager GetTokenManager()
        {
            TokenManager manager;
            manager = credential.IsInstance() ? new TokenManager(ServiceName, credential) : new TokenManager(ServiceName, UserName, Password);
            return manager;
        }

        public void Dispose()
        {   
            if (CallStack.IsInstance())
                CallStack.EndTimeStamp = DateTime.Now;
            Dispose(true);
        }

        /// <summary>
        /// Sets nettwork credentials to the container
        ///     </summary>
        /// <param name="credential"></param>
        public void SetNettworkCredentials(NetworkCredential credential)
        {
            Dispose(false);
            ClientFactory.Credentials.Windows.ClientCredential = credential;
            this.credential = credential;
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
                GC.SuppressFinalize(this);
            try
            {
                var channel = Client as IClientChannel;
                if (channel != null)
                {
                    channel.UnknownMessageReceived += channel_Faulted;
                    channel.Faulted += channel_Faulted;
                }
                CloseAndDisposeResources();
            }
            catch
            {
            }
            Client = default(T);
            Initialized = false;
            ReregisterForGC = true;
        }

        private void CloseAndDisposeResources()
        {
            if (Client.IsInstance())
                this.CloseProxy();
        }

        ~ServiceContainer()
        {
            Dispose(false);
        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            ServiceRootUrl = serviceRootUrl;
            return this;
        }

        public string GetUrl()
        {
            return Url;
        }
    }
}