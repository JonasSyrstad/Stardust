using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Stardust.Clusters;
using Stardust.Particles;
using Stardust.Starterkit.Proxy.Models;

namespace Stardust.Starterkit.Proxy.App_Start
{
    public class JoinGroupsPipeline : HubPipelineModule
    {
        ///// <summary>
        ///// Wraps a function that is called when a client connects to the <see cref="T:Microsoft.AspNet.SignalR.Hubs.HubDispatcher"/> for each
        /////             <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/> the client connects to. By default, this results in the <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/>'s
        /////             OnConnected method being invoked.
        ///// </summary>
        ///// <param name="connect">A function to be called when a client connects to a hub.</param>
        ///// <returns>
        ///// A wrapped function to be called when a client connects to a hub.
        ///// </returns>
        //public override Func<IHub, Task> BuildConnect(Func<IHub, Task> connect)
        //{
        //    connect = hub =>
        //        {
        //            try
        //            {
        //                var set = hub.Context.Headers["set"];
        //                var env = hub.Context.Headers["env"];
        //                Logging.DebugMessage("Connecting to: {0}", string.Format("{0}-{1}", set, env));
        //                hub.Groups.Add(hub.Context.ConnectionId, string.Format("{0}-{1}", set, env));
        //            }
        //            catch (Exception ex)
        //            {
        //                ex.Log();
        //            }
        //            return Task.FromResult(1);
        //        };
        //    return connect;
        //}

        /// <summary>
        /// Wraps a function that determines which of the groups belonging to the hub described by the <see cref="T:Microsoft.AspNet.SignalR.Hubs.HubDescriptor"/>
        ///             the client should be allowed to rejoin.
        ///             By default, clients will rejoin all the groups they were in prior to reconnecting.
        /// </summary>
        /// <param name="rejoiningGroups">A function that determines which groups the client should be allowed to rejoin.</param>
        /// <returns>
        /// A wrapped function that determines which groups the client should be allowed to rejoin.
        /// </returns>
        //public override Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups)
        //{
        //    rejoiningGroups = (descriptor, request, arg3) =>
        //        {
        //            try
        //            {
        //                var set = request.Headers["set"];
        //                var env = request.Headers["env"];
        //                Logging.DebugMessage("reconnecting to: {0}", string.Format("{0}-{1}", set, env));
        //                return new List<string> { string.Format("{0}-{1}", set, env) };
        //            }
        //            catch (Exception ex)
        //            {
        //                ex.Log();
        //                return new List<string>();
        //            }
        //        };
        //    return rejoiningGroups;
        //}

        /// <summary>
        /// Wraps a function to be called before a client subscribes to signals belonging to the hub described by the
        ///             <see cref="T:Microsoft.AspNet.SignalR.Hubs.HubDescriptor"/>. By default, the <see cref="T:Microsoft.AspNet.SignalR.Hubs.AuthorizeModule"/> will look for attributes on the
        ///             <see cref="T:Microsoft.AspNet.SignalR.Hubs.IHub"/> to help determine if the client is authorized to subscribe to method invocations for the
        ///             described hub.
        ///             The function returns true if the client is authorized to subscribe to client-side hub method
        ///             invocations; false, otherwise.
        /// </summary>
        /// <param name="authorizeConnect">A function that dictates whether or not the client is authorized to connect to the described Hub.
        ///             </param>
        /// <returns>
        /// A wrapped function that dictates whether or not the client is authorized to connect to the described Hub.
        /// </returns>
        public override Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect)
        {
            authorizeConnect = (descriptor, request) =>
                {
                    try
                    {
                        Logging.DebugMessage("Authorizing hub connection..");
                        var set = request.Headers["set"];
                        var env = request.Headers["env"];
                        var key = request.Headers["key"];
                        var localFile = ConfigCacheHelper.GetLocalFileName(set, env);
                        var token = ExtractToken(request);
                        var config = ConfigCacheHelper.GetConfigFromCache(localFile);
                        if (config == null)
                        {
                            ConfigCacheHelper.GetConfiguration(set, env, localFile);
                            config = ConfigCacheHelper.GetConfigFromCache(localFile);
                        }
                        var auth = config.TryValidateToken(env, token, key);
                        if (!auth) Logging.DebugMessage("Unauthorized access");
                            
                        else Logging.DebugMessage("Authorized '{0}'", request.User.Identity.Name);
                        return auth;
                    }
                    catch (Exception ex)
                    {
                        ex.Log();
                        return false;
                    }
                    
                };
            return authorizeConnect;
        }

        private static string ExtractToken(IRequest request)
        {
            return EncodingFactory.ReadFileText(Convert.FromBase64String(request.Headers["token"]));
        }
    }
}