using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;

namespace Stardust.Core.Service.Web.WebApiClient
{
    public class SwaggerServiceContainerFactory:IServiceContainerFactory
    {
        /// <summary>
        /// Creates a configured instance of a service container. 
        /// </summary>
        /// <typeparam name="TService">The service to wrap in a service container</typeparam><param name="runtime">The current <see cref="T:Stardust.Interstellar.IRuntime"/> instance</param><param name="serviceName">The service name for lookup in the configuration system</param><param name="scope">the OLM scope for the created instance.</param>
        /// <returns/>
        public IServiceContainer<TService> CreateContainer<TService>(IRuntime runtime, string serviceName, Scope scope = Scope.Context) where TService : class
        {
            return new SwaggerServiceContainer<TService>(serviceName);
        }
    }

    public class SwaggerServiceContainer<T> : IServiceContainer<T>
    {
        private readonly string serviceName;

        public SwaggerServiceContainer(string serviceName)
        {
            this.serviceName = serviceName;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initailizes the container to use UserName/Password for authentication
        /// </summary>
        /// <param name="useSecure"/>
        /// <returns/>
        public IServiceContainer<T> Initialize(bool useSecure = false)
        {
            return this;
        }

        /// <summary>
        /// Sets the bootstrap context used with Claims aware WCF services
        /// </summary>
        /// <param name="bootstrapContext"/>
        /// <returns/>
        public IServiceContainer<T> Initialize(BootstrapContext bootstrapContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the service credentials
        /// </summary>
        /// <param name="username"/><param name="password"/>
        /// <returns/>
        public IServiceContainer<T> SetCredentials(string username, string password)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and returns a WCF proxy object for the interface.
        /// </summary>
        /// <returns/>
        public T GetClient()
        {
            //client = new T();
            return default(T);
        }

        /// <summary>
        /// Sets nettwork credentials to the container
        /// </summary>
        /// <param name="credential"/>
        public void SetNettworkCredentials(NetworkCredential credential)
        {
            throw new NotImplementedException();
        }

        public IServiceContainer<T> SetServiceRoot(string serviceRootUrl)
        {
            throw new NotImplementedException();
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }

        public bool Initialized { get; private set; }
    }
}
