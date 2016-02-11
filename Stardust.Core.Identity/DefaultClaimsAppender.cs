using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Stardust.Core.CrossCutting;

namespace Stardust.Core.Identity
{
    class DefaultClaimsAppender : IClaimsAppender
    {
        /// <summary>
        /// Bound by DI container
        /// </summary>
        /// <param name="reader"></param>
        public DefaultClaimsAppender(IAttributeReader reader)
        {
            Reader = reader;
        }
        public ClaimsIdentity AppendClaimsTo(ClaimsIdentity claimsIdentity)
        {
            foreach (var claimToAppend in Reader.GetAttributesForUser(GetIdentity(claimsIdentity.Claims)))
                claimsIdentity.AddClaim(new Claim(claimToAppend.Key, claimToAppend.Value));
            return claimsIdentity;
        }

        private static string GetIdentity(IEnumerable<Claim> claimCollection)
        {
            var user = from c in claimCollection 
                       where c.Type == ClaimsDefinitions.ObjectSid 
                       select c;
            return user.First().Value;
        }

        public ClaimsIdentity RemoveClaimsFrom(ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity;
        }

        //public IClaimsAppender SetAttributeReader(IAttributeReader reader)
        //{
        //    Reader = reader;
        //    return this;
        //}

        public IAttributeReader Reader { get; set; }


        public ClaimsIdentity ModifyClaimsIn(ClaimsIdentity claimsIdentity)
        {
            var toChange = GetItemToChange(claimsIdentity);
            if (toChange.IsInstance())
                ChengeItem(claimsIdentity, toChange);
            return claimsIdentity;
        }

        private static Claim GetItemToChange(ClaimsIdentity claimsIdentity)
        {
            var item = from c in claimsIdentity.Claims
                       where c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                       select c;
            return item.FirstOrDefault();
        }

        private static void ChengeItem(ClaimsIdentity claimsIdentity, Claim toChange)
        {
            var newClaim = RewriteAndCreate(toChange);
            claimsIdentity.RemoveClaim(toChange);
            claimsIdentity.AddClaim(newClaim);
        }

        private static Claim RewriteAndCreate(Claim toChange)
        {
            return new Claim(toChange.Type, toChange.Value.Replace("(Admin)", " * "), toChange.Issuer);
        }

        public bool IsAuthorized(ClaimsIdentity claimsIdentity)
        {
            return true;
        }
    }
}