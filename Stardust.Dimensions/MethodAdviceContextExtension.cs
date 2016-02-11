using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;
using Stardust.Particles;

namespace Stardust.Dimensions
{
    internal static class MethodAdviceContextExtension
    {
        internal static bool IsAwaitable(this MethodAdviceContext ctx)
        {
            var methodInfo = ctx.TargetMethod as MethodInfo;

            
            return methodInfo != null && typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
        }

        internal static bool IsAwaitable(this AsyncMethodAdviceContext ctx)
        {
            var methodInfo = ctx.TargetMethod as MethodInfo;

            return methodInfo != null && typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
        }

        internal static bool IsAsync(this MethodAdviceContext ctx)
        {
            var methodInfo = ctx.TargetMethod as MethodInfo;
            var attrib = methodInfo.GetAttribute<AsyncStateMachineAttribute>();
            return (attrib != null);
        }

        internal static bool IsAsync(this AsyncMethodAdviceContext ctx)
        {
            var methodInfo = ctx.TargetMethod as MethodInfo;
            var attrib = methodInfo.GetAttribute<AsyncStateMachineAttribute>();
            return (attrib != null);
        }

        internal static async Task<object> ProceedAsync1(this MethodAdviceContext ctx)
        {
            ctx.Proceed();

            if (!ctx.HasReturnValue)
                return null;

            var task = ctx.ReturnValue as Task;

            if (task != null)
            {
                await task;

                // improve logic here we use MethodInfo.ReturnType (declared type) instead of task.GetType() which caused VoidTaskResult problem.
                var methodInfo = ctx.TargetMethod as MethodInfo;
                if (methodInfo != null && methodInfo.ReturnType.IsGenericType)
                {
                    return ((dynamic)task).Result;
                }

                return null;
            }

            return ctx.ReturnValue;
        }
    }
}