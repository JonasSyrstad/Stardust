

using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public abstract class ITypedClaimsIdentity<TInner> : ClaimsIdentity where TInner : TypedClaimsUser
    {
        public abstract ITypedClaimsIdentity<TInner> Initialize(ClaimsIdentity identity);
    }
}