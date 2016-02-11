using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    internal class ControllerTearDownActionFilter : AsyncControllerActionInvoker
    {
        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
            try
            {
                var result = base.InvokeExceptionFilters(controllerContext, filters, exception);
                var baseController = controllerContext.Controller as BaseController;
                if (baseController.IsInstance() && exception.IsInstance())
                {
                    try
                    {
                        result.HttpContext.Response.Headers.Add("X-Error", baseController.Runtime.InstanceId.ToString());
                        result.Exception = baseController.TearDown(exception);
                    }
                    catch (Exception ex)
                    {
                        ex.Log();
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }

        protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            var baseController = controllerContext.Controller as BaseController;
            try
            {
                var result = base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
                return baseController.IsInstance() ? baseController.TearDown(result) : result;
            }
            catch (Exception ex)
            {
                ex.Log();
                if (baseController.IsInstance())
                    throw baseController.TearDown(ex);
                throw;
            }


        }
    }
    
}