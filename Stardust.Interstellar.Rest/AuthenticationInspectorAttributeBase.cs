using System;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Interface)]
    public abstract class AuthenticationInspectorAttributeBase : Attribute, IAuthenticationInspector
    {
        public abstract IAuthenticationHandler GetHandler();
    }
}