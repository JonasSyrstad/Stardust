using System;
using System.Web.Mvc;
using System.Web.Routing;
using Stardust.Core.Wcf;

namespace Stardust.Core.Web.Mvc
{
    public class ControllerActivator : IControllerActivator
    {
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return (IController) controllerType.Activate();
        }
    }
}