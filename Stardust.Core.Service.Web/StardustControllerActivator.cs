using System;
using System.Web.Mvc;
using System.Web.Routing;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    internal class StardustControllerActivator : ControllerActivatorBase, IControllerActivator
    {
        IController IControllerActivator.Create(RequestContext requestContext, Type controllerType)
        {
            var controller = (IController)controllerType.Activate(Scope.PerRequest);
            InitializeController(requestContext, controller);
            return controller;
        }

        private void InitializeController(RequestContext requestContext, IController controller)
        {
            var controllerInitializer = controller as IStardustController;
            if (!controllerInitializer.IsInstance() || !controllerInitializer.DoInitializationOnActionInvocation)
                return;
            Initialize(requestContext.HttpContext.Request.Url, requestContext.HttpContext.Request.HttpMethod, controllerInitializer);

        }
    }
}