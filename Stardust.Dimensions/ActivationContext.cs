using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using ArxOne.MrAdvice.Advice;

namespace Stardust.Dimensions
{
    public sealed class ActivationContext : DynamicObject
    {
        /// <summary>
        /// Returns the enumeration of all dynamic member names. 
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return dynamicProperties.Keys;
        }

        private readonly ConcurrentDictionary<string, object> dynamicProperties = new ConcurrentDictionary<string, object>();
        private readonly AdviceContext context;

        public ActivationContext SetParameter<T>(string name, T value)
        {
            dynamicProperties.TryAdd(name, value);
            return this;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object value;
            if (dynamicProperties.TryGetValue(binder.Name, out value))
            {
                result = value;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return dynamicProperties.TryAdd(binder.Name, value);
        }

        public T GetParameter<T>(string name)
        {
            object value;
            if (dynamicProperties.TryGetValue(name, out value)) return (T)value;
            return default(T);
        }

        internal ActivationContext(AdviceContext context)
        {
            this.context = context;
        }

        public MemberInfo MethodInfo
        {
            get
            {
                var methodAdviceContext = context as MethodAdviceContext;
                if (methodAdviceContext != null)
                {
                    return methodAdviceContext.TargetMethod;
                }
                var propertyAdviceContext = context as PropertyAdviceContext;
                if (propertyAdviceContext != null)
                {
                    return propertyAdviceContext.TargetProperty;
                }
                return null;
            }
        }

        public string MemberName
        {
            get
            {
                return MethodInfo.Name;
            }
        }

        public ParameterInfo[] ParameterTypes
        {
            get
            {
                var methodAdviceContext = context as MethodAdviceContext;
                if (methodAdviceContext != null) return methodAdviceContext.TargetMethod.GetParameters();
                return new ParameterInfo[] { };
            }
        }

        public IList<object> Parameters
        {
            get
            {
                var methodAdviceContext = context as MethodAdviceContext;
                if (methodAdviceContext != null) return methodAdviceContext.Parameters;
                return new List<object>();
            }
        }

        public object Result
        {
            get
            {
                var methodAdviceContext = context as MethodAdviceContext;
                if (methodAdviceContext != null && methodAdviceContext.HasReturnValue) return ((MethodAdviceContext)context).ReturnValue;
                var propertyAdviceContext = context as PropertyAdviceContext;
                if (propertyAdviceContext != null && propertyAdviceContext.HasReturnValue) return propertyAdviceContext.ReturnValue;
                return null;
            }
        }

        public object Parent
        {
            get
            {
                return context.Target;
            }
        }

        public Type ParentType
        {
            get
            {
                return context.TargetType;
            }
        }

        public bool ExecuteAlternate { get; set; }

        public bool SkipTargetProcess { get; set; }

        public bool SwallowException { get; set; }

        public bool SkipAwait { get; set; }

        internal void Clean()
        {
            dynamicProperties.Clear();
        }
    }
}