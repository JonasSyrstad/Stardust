using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public class TypedClaimsUser : TypedClaims
    {
        public TypedClaimsUser()
        {
            Name = new TypedClaim<string>();
            EmailAddress = new TypedClaim<string>();
            Roles = new List<TypedClaim<string>>();
            Expiration = new TypedClaim<DateTime>();
            AuthenticationMethod = new TypedClaim<string>();
        }

        [TypedClaim(ClaimTypes.Name, false)]
        public TypedClaim<string> Name { get; private set; }
        
        [TypedClaim(ClaimTypes.Email, false)]
        public TypedClaim<string> EmailAddress { get; private set; }
        
        [TypedClaim(ClaimTypes.Role, true)]
        public List<TypedClaim<string>> Roles { get; private set; }
        
        [TypedClaim(ClaimTypes.Expiration, true)]
        public TypedClaim<DateTime> Expiration { get; private set; }
        
        [TypedClaim(ClaimTypes.AuthenticationMethod, false)]
        public TypedClaim<string> AuthenticationMethod { get; private set; }
        
        [TypedClaim(ClaimTypes.GroupSid, false)]
        public TypedClaim<string> GroupSid { get; private set; }

        [TypedClaim(ClaimsDefinitions.DistinguishedName, false)]
        public TypedClaim<string> DistinguishedName { get; private set; }

        [TypedClaim(ClaimsDefinitions.ObjectSid, false)]
        public TypedClaim<string> ObjectSid { get; private set; }

        [TypedClaim(ClaimsDefinitions.UserAccountControl, false)]
        public TypedClaim<string> UserAccountControl { get; private set; }
    }
}
