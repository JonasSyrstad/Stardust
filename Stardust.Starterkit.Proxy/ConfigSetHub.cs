using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Stardust.Particles;

namespace Stardust.Starterkit.Proxy
{
    [HubName("notificationHub")]
    public class ConfigSetHub : Hub
    {
        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnConnected()
        {
            var set = Context.Headers["set"];
            var env = Context.Headers["env"];
            //Logging.DebugMessage("connected to: {0}-{1}", set, env);
            ////Groups.Add(Context.ConnectionId, string.Format("{0}-{1}", set, env).ToLower());
            return base.OnConnected();
        }

        /// <summary>
        /// Called when the connection reconnects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnReconnected()
        {
            var set = Context.Headers["set"];
            var env = Context.Headers["env"];
            //Logging.DebugMessage("reconnectin to: {0}-{1}", set, env);
            ////Groups.Add(Context.ConnectionId, string.Format("{0}-{1}", set, env).ToLower());
            return base.OnReconnected();
        }

        /// <summary>
        /// Called when a connection disconnects from this hub gracefully or due to a timeout.
        /// </summary>
        /// <param name="stopCalled">true, if stop was called on the client closing the connection gracefully;
        ///             false, if the connection has been lost for longer than the
        ///             <see cref="P:Microsoft.AspNet.SignalR.Configuration.IConfigurationManager.DisconnectTimeout"/>.
        ///             Timeouts can be caused by clients reconnecting to another SignalR server in scaleout.
        ///             </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var set = Context.Headers["set"];
                var env = Context.Headers["env"];
                //Logging.DebugMessage("disconnecting from: {0}-{1}", set, env);
                ////Groups.Remove(Context.ConnectionId, string.Format("{0}-{1}", set, env).ToLower());
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return base.OnDisconnected(stopCalled);
        }

        public ConfigSetHub()
        { 
        }

        //public void ConfigSetUpdated(string id, string environment)
        //{
        //    Clients.Group(string.Format("{0}-{1}", id, environment).ToLower()).notify(id,environment);
        //}

        [HubMethodName("join")]
        public async Task Join(string id, string environment)
        {
            var groupName = string.Format("{0}-{1}", id, environment).ToLower();
            Logging.DebugMessage("New connection added to {0}",string.Format("{0}-{1}", id, environment));
            await Groups.Add(Context.ConnectionId, groupName);
            Clients.Caller.joinConfirmation("welcome {0} to {1}", Context.User.Identity.Name,groupName);
        }


    }
}