using System.Security.Claims;

namespace Stardust.Core.Identity
{
    public class TypedClaim<T> : Claim
    {
        public TypedClaim(string type, object value) : base(type, type) { Value = (T)value; }
        public TypedClaim() : base(ClaimTypes.Anonymous, "") { }
        public TypedClaim(Claim claim, string issuer) : base(claim.Type, claim.Value, claim.ValueType, issuer, claim.OriginalIssuer) { }
        public TypedClaim(string claimType, string value, string valueType) : base(claimType, value, valueType) { }
        public TypedClaim(string claimType, string value, string valueType, string issuer) : base(claimType, value, valueType, issuer) { }
        public TypedClaim(string claimType, string value, string valueType, string issuer, string originalIssuer) : base(claimType, value, valueType, issuer, originalIssuer) { }
        public TypedClaim(Claim claim) : base(claim.Type, claim.Value, claim.Value, claim.Issuer, claim.OriginalIssuer) { }
        public new T Value { get; internal set; }
    }
}