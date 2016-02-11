using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;

namespace Stardust.Dimensions
{
    public abstract class DimensionAttributeBase : AwaitableMethodAdviceBase, IPropertyAdvice
    {
        [DebuggerStepThrough]
        protected override async Task AdviseAsync(MethodAdviceContext context, Func<Task<object>> proceed)
        {
            var ctx = await DoBefore(context);
            if (ctx.ExecuteAlternate)
            {
                if (context.HasReturnValue && ctx.SkipTargetProcess)
                    context.ReturnValue = await AlternateProceedAsync(ctx);
                else await AlternateProceedAsync(ctx);
            }
            if (!ctx.SkipTargetProcess)
            {
                try
                {
                    await Proceed(proceed, ctx);
                }
                catch (Exception ex)
                {
                    HandleException(ctx, ex);
                    if (!ctx.SwallowException) throw;
                }
            }
            ctx = await DoAfter(context, ctx);
            ctx.Clean();
        }

        private void HandleException(ActivationContext ctx, Exception ex)
        {
            ctx.SetParameter("Exception", ex);
            OnException(ctx, ex);
        }

        private static async Task Proceed(Func<Task<object>> proceed, ActivationContext ctx)
        {
            if (!ctx.SkipAwait)
            {
                await proceed();
            }
            else
            {
                proceed();
            }
        }

        private async Task<ActivationContext> DoAfter(MethodAdviceContext context, ActivationContext ctx)
        {
            if (context.IsAwaitable())
            {
                ctx = await AfterProcesssingAsync(ctx);
            }
            else
            {
                ctx = AfterProcesssing(ctx);
            }
            return ctx;
        }

        private async Task<ActivationContext> DoBefore(MethodAdviceContext context)
        {
            ActivationContext ctx;
            if (context.IsAwaitable())
            {
                ctx = await BeforeProcessingAsync(new ActivationContext(context));
            }
            else
            {
                ctx = BeforeProcessing(new ActivationContext(context));
            }
            return ctx;
        }

        protected virtual void OnException(ActivationContext ctx, Exception exception)
        {

        }

        /// <summary>
        /// Uses the sync implementation as default
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected virtual Task<object> AlternateProceedAsync(ActivationContext ctx)
        {
            return Task.FromResult(AlternateProceed(ctx));
        }

        protected virtual object AlternateProceed(ActivationContext ctx)
        {
            return null;
        }

        /// <summary>
        /// Uses the sync implementation as default
        /// </summary>
        /// <param name="context"></param>
        /// <returns>return the incoming context</returns>
        protected virtual Task<ActivationContext> AfterProcesssingAsync(ActivationContext ctx)
        {
            return Task.FromResult(AfterProcesssing(ctx));
        }

        /// <summary>
        /// Uses the sync implementation as default
        /// </summary>
        /// <param name="context"></param>
        /// <returns>return the incoming context</returns>
        protected virtual Task<ActivationContext> BeforeProcessingAsync(ActivationContext ctx)
        {
            return Task.FromResult(BeforeProcessing(ctx));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>return the incoming context</returns>
        protected abstract ActivationContext AfterProcesssing(ActivationContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>return the incoming context</returns>
        protected abstract ActivationContext BeforeProcessing(ActivationContext context);

        void IPropertyAdvice.Advise(PropertyAdviceContext context)
        {
            var ctx = BeforeProcessing(new ActivationContext(context));
            if (ctx.ExecuteAlternate)
            {
                if (context.IsGetter && ctx.SkipTargetProcess)
                {
                    var task = AlternateProceed(ctx);
                    context.Value = task;
                }
                else AlternateProceed(ctx);
            }
            if (!ctx.SkipTargetProcess)
            {
                try
                {
                    context.Proceed();
                }
                catch (Exception ex)
                {
                    HandleException(ctx,ex);
                    throw;
                }
            }
            ctx = AfterProcesssing(ctx);
            ctx.Clean();
        }
    }
}