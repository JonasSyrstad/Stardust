using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace Stardust.Core.Identity
{
    [Serializable]
    public class TypedClaimsIdentity : TypedClaimsIdentity<TypedClaimsUser>
    {
        public TypedClaimsIdentity() { }

        public TypedClaimsIdentity(ClaimsIdentity identity)
            : base(identity)
        { }
    }

    [Serializable]
    public class TypedClaimsIdentity<T> : ITypedClaimsIdentity<T> where T : TypedClaimsUser, new()
    {

        private List<Claim> ClaimsPrivate;
        private T UserPrivate;

        public TypedClaimsIdentity() { }

        public TypedClaimsIdentity(ClaimsIdentity identity)
        {
            Initialize(identity);
        }

        public override sealed ITypedClaimsIdentity<T> Initialize(ClaimsIdentity identity)
        {
            UserPrivate = new T();
            ClaimsPrivate = new List<Claim>(identity.Claims);
            Actor= identity.Actor;
            Update();
            return this;
        }

        public void Update()
        {
            UserPrivate.Update(ClaimsPrivate);
        }

        public T User
        {
            get
            {
                Update();
                return UserPrivate;
            }
        }

        public SecurityToken BootstrapToken { get; set; }

        public ClaimsIdentity Copy()
        {
            var claimsIdentity = new ClaimsIdentity(AuthenticationType);
            if (Claims != null)
            {
                var copyTo=new Claim[ClaimsPrivate.Count];
                ClaimsPrivate.CopyTo(copyTo);
                claimsIdentity.AddClaims(copyTo);
            }
            claimsIdentity.Label = Label;
            claimsIdentity.BootstrapContext = BootstrapToken;
            return claimsIdentity;
        }
    }
}