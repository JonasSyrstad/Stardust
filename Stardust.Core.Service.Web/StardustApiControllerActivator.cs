using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    internal class StardustApiControllerActivator : ControllerActivatorBase, IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var controller= (IHttpController) controllerType.Activate(Scope.PerRequest);
            InitalizeController(request, controller);
            return controller;

        }
        private void InitalizeController(HttpRequestMessage requestContext, IHttpController controller)
        {
            var controllerInitializer = controller as IStardustController;
            if (!controllerInitializer.IsInstance() || !controllerInitializer.DoInitializationOnActionInvocation)
                return;
            Initialize(requestContext.RequestUri, requestContext.Method.Method, controllerInitializer);
        }
    }
}