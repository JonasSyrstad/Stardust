using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;

namespace Stardust.Dimensions
{
    public abstract class AwaitableMethodAdviceBase : Attribute, IMethodAdvice
    {
        [DebuggerStepThrough]
        void IMethodAdvice.Advise(MethodAdviceContext context)
        {
            if (context.IsAwaitable() && context.IsAsync())
                AdviseAsync(context, async () => await context.ProceedAsync1());
            else if (context.IsAwaitable())
                AdviseAsync(context, context.ProceedAsync1);
            else
                AdviseSync(context, () =>
                    {
                        context.Proceed();
                        return 1;
                    });
        }
        [DebuggerStepThrough]
        protected virtual void AdviseSync(MethodAdviceContext context, Func<object> proceed)
        {
            AdviseAsync(context, () => Task.FromResult(proceed()));
        }

        [DebuggerStepThrough]
        protected abstract Task AdviseAsync(MethodAdviceContext context, Func<Task<object>> proceed);
    }
}