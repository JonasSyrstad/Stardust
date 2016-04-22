using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;
using ArxOne.MrAdvice.Threading;
using Stardust.Interstellar.Trace;
using Stardust.Particles;
#pragma warning disable 4014

namespace Stardust.Dimensions
{
    #region PureAsync
    ///<summary>
    ///Wraps the stardust TracerFactory.StartTrace in an weaved aspect. If you want to set additional debug info to the callstack item use the TracerFactory directly
    ///</summary>
    ///<remarks>
    ///Do not add this on class level when doing recursive calls or looping through big collections. The trace log becomes quite large and may cause performance issues.
    ///</remarks>
    ///<param name="context">The method advice context.</param>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    [AspectPriority(AspectPriority.Highest)]
    public class TraceAttribute : Attribute, IMethodAdvice, IMethodAsyncAdvice, IPropertyAdvice
    {
        private readonly string info;

        [DebuggerHidden]
        public bool TreatExceptionAsInformational { get; set; }

        /// <summary>
        /// Does not await the async operation when executing the after call part of the aspect.
        /// </summary>
        [DebuggerHidden]
        public bool DisableAwait { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public TraceAttribute()
        {
        }

        public TraceAttribute(string info)
        {
            this.info = info;
        }

        /// <summary>
        /// Wraps the stardust TracerFactory.StartTrace in an weaved aspect. If you want to set additional debug info to the callstack item use the TracerFactory directly
        /// </summary>
        /// <param name="context">The method advice context.</param>
        [DebuggerStepThrough]
        [DebuggerHidden]
        public void Advise(PropertyAdviceContext context)
        {

            using (var t = TracerFactory.StartTracer(context.TargetType, string.Format("{0}_{1}", context.IsGetter ? "get" : "set", context.TargetProperty.Name)))
            {
                if (info.ContainsCharacters()) t.SetAdidtionalInformation(info);
                try
                {

                    context.Proceed();
                }
                catch (Exception ex)
                {
                    if (TreatExceptionAsInformational) t.SetException(ex);
                    else t.SetErrorState(ex);
                    throw;
                }
            }
        }



        /// <summary>
        /// Wraps the stardust TracerFactory.StartTrace in an weaved aspect. If you want to set additional debug info to the callstack item use the TracerFactory directly
        /// </summary>
        /// <param name="context">The method advice context.</param>
        [DebuggerStepThrough]
        [DebuggerHidden]
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            using (var t = TracerFactory.StartTracer(context.TargetType, context.TargetMethod.Name))
            {
                if (info.ContainsCharacters()) t.SetAdidtionalInformation(info);
                try
                {
                    if (!context.IsTargetMethodAsync) Task.Run(() => context.ProceedAsync()).Wait();
                    else await context.ProceedAsync();
                }
                catch (AggregateException aex)
                {
                    if (aex.InnerExceptions == null)
                    {
                        SetErrorInfo(t, aex.Flatten());
                        throw aex.Flatten();
                    };
                    if (aex.InnerExceptions.Count > 1)
                    {
                        SetErrorInfo(t, aex.Flatten());
                        throw aex.Flatten();
                    }
                    var ex = aex.InnerException;
                    SetErrorInfo(t, ex);
                    throw ex;
                }
                catch (Exception ex)
                {
                    SetErrorInfo(t, ex);
                    throw;
                }
            }
        }

        private void SetErrorInfo(ITracer t, Exception ex)
        {
            if (TreatExceptionAsInformational)
            {
                t.SetException(ex);
            }
            else
            {
                t.SetErrorState(ex);
            }
        }

        private static Exception TryUnwrapException(Task r)
        {
            if (r.Exception != null && r.Exception.InnerExceptions.Count == 1) return r.Exception.InnerException;
            return r.Exception;
        }

        /// <summary>
        /// Implements advice logic.
        ///             Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        public void Advise(MethodAdviceContext context)
        {
            using (var t = TracerFactory.StartTracer(context.TargetType, context.TargetMethod.Name))
            {
                try
                {
                    context.Proceed();
                }
                catch (Exception ex)
                {
                    if (TreatExceptionAsInformational)
                    {
                        t.SetException(ex);
                    }
                    else
                        t.SetErrorState(ex);
                }
            }
        }
    }
    #endregion

    #region workarrroundAsync
    //[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    //[AspectPriority(AspectPriority.Highest)]
    //public class TraceAttribute : AwaitableMethodAdviceBase, IPropertyAdvice
    //{
    //    private readonly string info;

    //    [DebuggerHidden]
    //    public bool TreatExceptionAsInformational { get; set; }

    //    /// <summary>
    //    /// Does not await the async operation when executing the after call part of the aspect.
    //    /// </summary>
    //    [DebuggerHidden]
    //    public bool DisableAwait { get; set; }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
    //    /// </summary>
    //    public TraceAttribute()
    //    {
    //    }

    //    public TraceAttribute(string info)
    //    {
    //        this.info = info;
    //    }

    //    /// <summary>
    //    /// Wraps the stardust TracerFactory.StartTrace in an weaved aspect. If you want to set additional debug info to the callstack item use the TracerFactory directly
    //    /// </summary>
    //    /// <param name="context">The method advice context.</param>
    //    [DebuggerStepThrough]
    //    [DebuggerHidden]
    //    public void Advise(PropertyAdviceContext context)
    //    {
    //        using (var t = TracerFactory.StartTracer(context.TargetType, string.Format("{0}_{1}", context.IsGetter ? "get" : "set", context.TargetProperty.Name)))
    //        {
    //            if (info.ContainsCharacters()) t.SetAdidtionalInformation(info);
    //            try
    //            {
    //                context.Proceed();
    //            }
    //            catch (Exception ex)
    //            {
    //                if (TreatExceptionAsInformational) t.SetException(ex);
    //                else t.SetErrorState(ex);
    //                throw;
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Wraps the stardust TracerFactory.StartTrace in an weaved aspect. If you want to set additional debug info to the callstack item use the TracerFactory directly
    //    /// </summary>
    //    /// <param name="context">The method advice context.</param>
    //    [DebuggerStepThrough]
    //    [DebuggerHidden]
    //    protected override async Task AdviseAsync(MethodAdviceContext context, Func<Task<object>> proceed)
    //    {
    //        using (var t = TracerFactory.StartTracer(context.TargetType, context.TargetMethod.Name))
    //        {
    //            if (info.ContainsCharacters()) t.SetAdidtionalInformation(info);
    //            try
    //            {
    //                if (DisableAwait)
    //                    proceed();
    //                else
    //                    await proceed();
    //            }
    //            catch (AggregateException aex)
    //            {
    //                if (aex.InnerExceptions.Count < 2) throw;
    //                var ex = aex.InnerException;
    //                if (TreatExceptionAsInformational) t.SetException(ex);
    //                else t.SetErrorState(ex);
    //                throw ex;
    //            }
    //            catch (Exception ex)
    //            {
    //                if (TreatExceptionAsInformational) t.SetException(ex);
    //                else t.SetErrorState(ex);
    //                throw;
    //            }
    //        }
    //    }
    //}
    #endregion
}
