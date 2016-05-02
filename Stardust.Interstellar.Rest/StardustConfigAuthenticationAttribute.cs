using System;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class StardustConfigAuthenticationAttribute: AuthenticationInspectorAttributeBase
    {
        public override IAuthenticationHandler GetHandler()
        {
            return new StardustConfigAuthentication();
        }
    }
}