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

        /// <summary>
        /// Invokes the asynchronous action method by using the specified controller context, action name, callback method, and state.
        /// </summary>
        /// <returns>
        /// An object that contains the result of an asynchronous operation.Implements<see cref="M:System.Web.Mvc.Async.IAsyncActionInvoker.BeginInvokeAction(System.Web.Mvc.ControllerContext,System.String,System.AsyncCallback,System.Object)"/>
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="actionName">The name of the action.</param><param name="callback">The callback method.</param><param name="state">An object that contains information to be used by the callback method. This parameter can be null.</param>
        public override IAsyncResult BeginInvokeAction(ControllerContext controllerContext, string actionName, AsyncCallback callback, object state)
        {

            var result= base.BeginInvokeAction(controllerContext, actionName, callback, state);
            return result;
        }

        

        /// <summary>
        /// Cancels the action.
        /// </summary>
        /// <returns>
        /// true if the action was canceled; otherwise, false.
        /// </returns>
        /// <param name="asyncResult">The user-defined object that qualifies or contains information about an asynchronous operation.</param>
        public override bool EndInvokeAction(IAsyncResult asyncResult)
        {
            return base.EndInvokeAction(asyncResult);
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