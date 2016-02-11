using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Stardust.Interstellar;
using Stardust.Interstellar.Tasks;
using Stardust.Particles;

namespace Stardust.Core.Service.Web
{
    internal class ApiTeardownActionFilter : ApiControllerActionInvoker
    {
        public override Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            
            var task = base.InvokeActionAsync(actionContext, cancellationToken);
            try
            {
                AddTearDownTask(actionContext, cancellationToken, task);
            }
            catch (Exception ex)
            {
                var baseController = actionContext.ControllerContext.Controller as BaseApiController;
                if (baseController != null)
                    baseController.TearDown(ex);
                throw;
            }
            return task;
        }

        private static void AddTearDownTask(HttpActionContext actionContext, CancellationToken cancellationToken, Task<HttpResponseMessage> task)
        {
            var baseController = actionContext.ControllerContext.Controller as BaseApiController;
            if (baseController != null)
            {
                task.ContinueWith(t =>
                {
                    if (task.IsCanceled)
                    {
                        baseController.TearDown("Canceled");
                    }
                    else if (t.IsCompleted)
                    {
                        //baseController.TearDown(t.Result);
                    }
                    else if (t.IsFaulted)
                    {
                        AddSupportCode(actionContext, baseController);
                        baseController.TearDown(t.Exception);
                    }
                }, cancellationToken);
            }
        }

        private static void AddSupportCode(HttpActionContext actionContext, BaseApiController baseController)
        {
            try
            {
                string supportCode;
                if (TryGetSupportCode(baseController.Runtime, out supportCode))
                {
                    actionContext.Response.Headers.Add("X-Error", supportCode);
                }
                else
                {
                    actionContext.Response.Headers.Add("X-Error", baseController.Runtime.InstanceId.ToString());
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        internal static bool TryGetSupportCode(IRuntime runtime, out string supportCode)
        {
            if (runtime == null || runtime.GetStateStorageContainer() == null)
            {
                supportCode = null;
                return false;
            }
            StateStorageItem item;
            var result = runtime.GetStateStorageContainer().TryGetItem("supportCode", out item);
            if (item != null && item.Value != null) supportCode = (string)item.Value;
            else supportCode = null;
            return result;

        }
    }

}