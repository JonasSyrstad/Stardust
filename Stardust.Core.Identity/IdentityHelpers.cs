using System.Linq;
using System.Security.Claims;
using System.Web;
using Stardust.Core.FactoryHelpers;

namespace Stardust.Core.Identity
{
    public static class IdentityHelpers
    {
        /// <summary>
        /// Use ClaimsDefinitions for common claims types or provide the correct namespace 
        /// </summary>
        public static string GetSingleClaim(this ClaimsIdentity identity, string claimsType)
        {
            var value = from v in identity.Claims
                        where v.Type == claimsType
                        select v.Value;
            return value.First();
        }

        /// <summary>
        /// Use ClaimTypes for common claims types or provide the correct namespace 
        /// </summary>
        public static string[] GetClaims(this ClaimsIdentity identity, string claimsType)
        {
            var value = from v in identity.Claims where v.Type == claimsType select v.Value;
            return value.ToArray();
        }

        public static ITypedClaimsIdentity<TypedClaimsUser> GetClaimsUser()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            return identity.ConvertToTypedClaimsIdentity<TypedClaimsUser>();
        }

        public static ITypedClaimsIdentity<T> GetClaimsUser<T>() where T : TypedClaimsUser
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            return identity.ConvertToTypedClaimsIdentity<T>();
        }

        public static ITypedClaimsIdentity<T> ConvertToTypedClaimsIdentity<T>(this ClaimsIdentity identity) where T : TypedClaimsUser
        {
            return Resolver.Activate<ITypedClaimsIdentity<T>>(t => t.Initialize(identity));
        }

        public static ITypedClaimsIdentity<TypedClaimsUser> ConvertToTypedClaimsIdentity(this ClaimsIdentity identity)
        {
            return
                Resolver.Activate<ITypedClaimsIdentity<TypedClaimsUser>>(t => t.Initialize(identity));
        }
    }
}