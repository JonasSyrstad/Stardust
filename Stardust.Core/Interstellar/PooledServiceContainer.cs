using System;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Stardust.Core.Pool;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public interface IPooledServiceContainer<T> : IServiceContainer<T>
    {
        string PoolName { get; }
        string TypeName { get; }
        int Id { get; }
        bool Used { get; }
    }

    public class PooledServiceContainer<T> : ConnectionStringPoolableBase, IPooledServiceContainer<T>
    {
        private ServiceContainer<T> Container;

        protected internal override void Dispose(bool disposing)
        {
            if (Container.IsInstance())
                Container.Dispose();
            if (disposing)
                GC.SuppressFinalize(this);
        }

        public void InitializeContainer(ChannelFactory<T> clientFactory, string serviceName)
        {
            if (Container.IsInstance())
            {
                if (Container.Initialized) return;
            }
            Container = new ServiceContainer<T>();
            Container.InitializeContainer(clientFactory, ConnectionString, serviceName);
        }


        public void SetNettworkCredentials(System.Net.NetworkCredential credential)
        {
            Container.SetNettworkCredentials(credential);
        }

        public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
        {
            return Container.ExecuteMethod(executor);
        }

        public IServiceContainer<T> Initialize(bool useSecure = false)
        {
            return Container.Initialize(useSecure);
        }

        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            return Container.Initialize(bootstrapContext); ;
        }

        public T GetClient()
        {
            return Container.GetClient();
        }

        public bool Initialized
        {
            get
            {
                if (Container.IsNull()) return false;
                return Container.Initialized;
            }
        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            Container.SetServiceRoot(serviceRootUrl);
            return this;
        }

        public string GetUrl()
        {
            return Container.GetUrl();
        }


        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            Container.SetCredentials(username, password);
            return this;
        }
    }
}