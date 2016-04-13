using System;
using System.IdentityModel.Tokens;
using System.Net;
using Stardust.Core.Security;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.DefaultImplementations
{
    /// <summary>
    /// Unit test mock tool for ServiceContainers. Register the mock service implementation with the IOC in test initialization
    /// </summary>
    public sealed class IocBasedServiceContainerFactory : IServiceContainerFactory
    {
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context) where TService : class
        {
            return new ResolverBasedServiceContainer<TService>();
        }

        private class ResolverBasedServiceContainer<T> : IServiceContainer<T>
        {
            private T Client;
            private BootstrapContext BootstrapToken;
            private BasicCredentials BasicCredentials;
            private NetworkCredential NettworkCredentials;

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (disposing) GC.SuppressFinalize(this);
                Client.TryDispose();
                Client = default(T);
            }

            ~ResolverBasedServiceContainer()
            {
                Dispose(false);
            }


            public bool Initialized
            {
                get { return true; }
            }

            public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
            {
                return executor();
            }

            public IServiceContainer<T> Initialize(bool useSecure = false)
            {
                return this;
            }

            public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
            {
                BootstrapToken = bootstrapContext;
                return this;
            }

            public IServiceContainer<T> SetCredentials(string username, string password)
            {
                BasicCredentials = new BasicCredentials(username, new EncryptionKeyContainer(password));
                return this;
            }

            public T GetClient()
            {
                if (Client != null) return Client;
                Client = Resolver.Activate<T>();
                var secClient = Client as ISecurityEnabledService;
                if (secClient.IsInstance())
                {
                    //TODO: add security related settings.
                }
                return Client;
            }

            public void SetNettworkCredentials(NetworkCredential credential)
            {
                NettworkCredentials = credential;
            }

            public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
            {
                return this;
            }

            public string GetUrl()
            {
                return string.Format("http://unittest/{0}", typeof(T).Name);
            }
        }
    }
}