using System;
using System.Linq;
using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public class TypedClaimsAuthenticationManager : ClaimsAuthenticationManager
    {
        private readonly IClaimsAppender ClaimsAppender = ClaimsManipulatorFactory.GetClaimsAppender();

        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
                return base.Authenticate(resourceName, incomingPrincipal);
            //for (var i = 0; i < incomingPrincipal.Identities.Count(); i++)
            //{
            //    var ci = ManipulateClaims(incomingPrincipal.Identities[i], ClaimsAppender);
            //    if (IsUnAuthorized(ci))
            //        return base.Authenticate(resourceName, new ClaimsPrincipal());
            //    incomingPrincipal.Identities[i] = Resolver.Resolve<ClaimsIdentity>().Activate(new object[] { ci });
            //}
            ManipulateClaims(incomingPrincipal.Identities.First(), ClaimsAppender);
            return base.Authenticate(resourceName, incomingPrincipal);
        }

        private static ClaimsIdentity ManipulateClaims(ClaimsIdentity claimsIdentity, IClaimsAppender claimsAppender)
        {
            var ci = claimsIdentity;
            ci = RemoveClaims(claimsAppender, ci);
            ci = ModifyClaims(claimsAppender, ci);
            ci = AppendClaims(claimsAppender, ci);
            return ci;
        }

        private static ClaimsIdentity RemoveClaims(IClaimsAppender claimsAppender, ClaimsIdentity ci)
        {
            try
            {
                ci = claimsAppender.RemoveClaimsFrom(ci);
            }
            catch (NotImplementedException) { }
            return ci;
        }

        private static ClaimsIdentity ModifyClaims(IClaimsAppender claimsAppender, ClaimsIdentity ci)
        {
            try
            {
                ci = claimsAppender.ModifyClaimsIn(ci);
            }
            catch (NotImplementedException) { }
            return ci;
        }

        private static ClaimsIdentity AppendClaims(IClaimsAppender claimsAppender, ClaimsIdentity ci)
        {
            try
            {
                ci = claimsAppender.AppendClaimsTo(ci);
            }
            catch (NotImplementedException) { }
            return ci;
        }
    }
}