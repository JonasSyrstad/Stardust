using System;

namespace Stardust.Core.Identity
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class TypedClaimAttribute : Attribute
    {
        readonly string claimType;
        readonly bool isCollection;

        public TypedClaimAttribute(string claimType, bool isCollection)
        {
            this.claimType = claimType;
            this.isCollection = isCollection;
        }

        public string ClaimType
        {
            get { return claimType; }
        }

        public bool IsCollection { get { return isCollection; } }
    }
}