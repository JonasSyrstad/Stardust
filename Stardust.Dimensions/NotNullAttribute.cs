using System;
using ArxOne.MrAdvice.Advice;
using Stardust.Particles;

namespace Stardust.Dimensions
{
    /// <summary>
    /// Use this to generate a meaningfull error message when a mandatory parameter is null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    [AspectPriority(AspectPriority.Low)]
    public class NotNullAttribute : Attribute, IParameterAdvice, IPropertyAdvice
    {
        private bool throwOnGet;

        /// <summary>
        /// Implements advice logic.
        ///             Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The parameter advice context.</param>
        void IParameterAdvice.Advise(ParameterAdviceContext context)
        {
            if (context.Value == null && !context.IsOut)
            {
                Logging.DebugMessage("Parameter {0} is null on {1}", context.TargetParameter.Name, context.TargetType.FullName);
                throw new ArgumentNullException(context.TargetParameter.Name, string.Format("Parameter '{0}' in method {2} is null on '{1}'", context.TargetParameter.Name, context.TargetType.FullName, context.TargetParameter.Member.Name));
            }
            context.Proceed();
        }

        /// <summary>
        /// Implements advice logic.
        ///             Usually, advice must invoke context.Proceed()
        /// </summary>
        /// <param name="context">The method advice context.</param>
        void IPropertyAdvice.Advise(PropertyAdviceContext context)
        {
            if (context.IsSetter && context.Value == null)
            {
                Logging.DebugMessage("Property '{0}' cannot be null on '{1}'{2}Parent object: {2}{3}", context.TargetProperty.Name, context.TargetType.FullName, Environment.NewLine, context.Target == null ? "" : context.Target.ToString());
                throw new ArgumentNullException(context.TargetProperty.Name, string.Format("Property '{0}' cannot be null on '{1}'", context.TargetProperty.Name, context.TargetType.FullName));
            }
            context.Proceed();
            if (context.IsGetter && CheckOnGet && (!context.HasReturnValue || context.ReturnValue == null))
            {
                Logging.DebugMessage("Property '{0}' cannot be null on '{1}'{2}Parent object: {2}{3}", context.TargetProperty.Name, context.TargetType.FullName, Environment.NewLine, context.Target == null ? "" : context.Target.ToString());
                if (ThrowOnGet)
                    throw new ArgumentNullException(context.TargetProperty.Name, string.Format("Property '{0}' cannot be null on '{1}'", context.TargetProperty.Name, context.TargetType.FullName));
            }
        }

        /// <summary>
        /// Throw exception on null read
        /// </summary>
        public bool ThrowOnGet
        {
            get
            {
                return throwOnGet;
            }
            set
            {
                if (value) CheckOnGet = true;
                throwOnGet = value;
            }
        }

        /// <summary>
        /// Performe a null check when reading the property
        /// </summary>
        public bool CheckOnGet { get; set; }
    }
}