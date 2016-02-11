using System;
using System.IdentityModel.Tokens;
using System.Net;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar
{
    public interface IServiceContainer<T> : IDisposable
    {
        bool Initialized { get; }
        TOut ExecuteMethod<TOut>(Func<TOut> executor) where TOut : IResponseBase;

        /// <summary>
        /// Initailizes the container to use UserName/Password for authentication
        /// </summary>
        /// <param name="useSecure"></param>
        /// <returns></returns>
        IServiceContainer<T> Initialize(bool useSecure = false);

        /// <summary>
        /// Sets the bootstrap context used with Claims aware WCF services
        /// </summary>
        /// <param name="bootstrapContext"></param>
        /// <returns></returns>
        IServiceContainer<T> Initialize(BootstrapContext bootstrapContext);

        /// <summary>
        /// Sets the service credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IServiceContainer<T> SetCredentials(string username, string password);

        /// <summary>
        /// Creates and returns a WCF proxy object for the interface.
        /// </summary>
        /// <returns></returns>
        T GetClient();

        /// <summary>
        /// Sets nettwork credentials to the container
        /// </summary>
        /// <param name="credential"></param>
        void SetNettworkCredentials(NetworkCredential credential);

        IServiceContainer<T> SetServiceRoot(string serviceRootUrl);
        string GetUrl();
    }
}