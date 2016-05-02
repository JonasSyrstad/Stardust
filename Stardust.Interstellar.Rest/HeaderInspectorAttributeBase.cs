using System;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Method , AllowMultiple = true)]
    public abstract class HeaderInspectorAttributeBase : Attribute, IHeaderInspector
    {
        public abstract IHeaderHandler[] GetHandlers();
    }
}