using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;
using Stardust.Particles;

namespace Stardust.Dimensions
{
    /// <summary>
    /// Logs any uncought exceptions to the error log.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    [AspectPriority(AspectPriority.Medium)]
    public class LogExceptionAttribute : AwaitableMethodAdviceBase, IPropertyAdvice
    {
        /// <summary>
        /// Logs any uncought exceptions to the error log.
        /// </summary>
        /// <param name="context">The method advice context.</param>
        [DebuggerStepThrough]
        protected override async Task AdviseAsync(MethodAdviceContext context, Func<Task<object>> proceed)
        {
            try
            {
                await proceed();
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }

        /// <summary>
        /// Logs any uncought exceptions to the error log.
        /// </summary>
        /// <param name="context">The method advice context.</param>
        [DebuggerStepThrough]
        void IPropertyAdvice.Advise(PropertyAdviceContext context)
        {
            try
            {
                context.Proceed();
            }
            catch (NullReferenceException ex)
            {
                ex.Log(string.Format("Null reference in {2}.{0}.[{1}]", context.TargetProperty.Name, context.IsGetter ? "get" : "set", context.TargetType.FullName));
                throw;
            }
            catch (Exception ex)
            {
                ex.Log(context.IsGetter ? "Getter" : "Setter");
                throw;
            }
        }
    }
}