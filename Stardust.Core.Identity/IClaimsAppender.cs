

using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public interface IClaimsAppender
    {
        //IClaimsAppender SetAttributeReader(IAttributeReader reader);
        ClaimsIdentity AppendClaimsTo(ClaimsIdentity claimsIdentity);
        ClaimsIdentity RemoveClaimsFrom(ClaimsIdentity claimsIdentity);
        ClaimsIdentity ModifyClaimsIn(ClaimsIdentity claimsIdentity);
        bool IsAuthorized(ClaimsIdentity claimsIdentity);
    }
}