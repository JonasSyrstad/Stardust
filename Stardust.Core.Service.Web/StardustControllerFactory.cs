using System.Web.Mvc;
using System.Web.Routing;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    internal sealed class StardustControllerFactory : DefaultControllerFactory
    {
        [Using]
        internal StardustControllerFactory(IControllerActivator activator)
            : base(activator)
        {

        }

        public override IController CreateController(RequestContext context, string controllerName)
        {

            try
            {
                var controller = base.CreateController(context, controllerName);
                return ReplaceActionInvoker(controller);
            }
            catch (System.Exception ex)
            {
                ex.Log();
                Logging.DebugMessage("Unable to locate {0}", controllerName);
                return null;
            }

        }



        private IController ReplaceActionInvoker(IController controller)
        {
            var mvcController = controller as BaseController;
            if (mvcController != null)
                mvcController.ActionInvoker = new ControllerTearDownActionFilter(); return controller;

        }

    }

    
}