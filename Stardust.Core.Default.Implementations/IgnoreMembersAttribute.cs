using System;

namespace Stardust.Core.Default.Implementations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IgnoreMembersAttribute : Attribute,IIgnoreAttribute
    {
        public string NameOfProperties { get; set; }

        public IgnoreMembersAttribute(string nameOfProperties)
        {
            NameOfProperties = nameOfProperties;
        }

        bool IIgnoreAttribute.IsIgnored(string memberName)
        {
            return NameOfProperties.Contains(memberName);
        }
    }
}