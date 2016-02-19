using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Stardust.Starterkit.Configuration.Web.Notification
{
    [HubName("configSetHub")]
    public class ConfigSetHub : Hub
    {
        [HubMethodName("changed")]
        public void ConfigSetUpdated(string id, string environment)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ConfigSetHub>();

            context.Clients.All.changed(id, environment);
        }
    }
}