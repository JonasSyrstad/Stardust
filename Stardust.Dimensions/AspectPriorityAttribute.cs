using System;
using ArxOne.MrAdvice.Annotation;

namespace Stardust.Dimensions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AspectPriorityAttribute : PriorityAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ArxOne.MrAdvice.Annotation.PriorityAttribute"/> class.
        ///             Assigns a low priority aspect
        /// </summary>
        public AspectPriorityAttribute()
            : base((int)AspectPriority.Low)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ArxOne.MrAdvice.Annotation.PriorityAttribute"/> class.
        ///             Assigns a priority
        /// </summary>
        /// <param name="level">The level.</param>
        public AspectPriorityAttribute(AspectPriority level)
            : base((int)level)
        {
        }
    }
}