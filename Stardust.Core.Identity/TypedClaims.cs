using System.Collections.Generic;
using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public class TypedClaims
    {
        internal void Update(IEnumerable<Claim> claims)
        {
            ReflectedPropertiesSetter.Update(claims, this);
        }
    }
}