using System;

namespace Stardust.Interstellar.Rest
{

    [AttributeUsage(AttributeTargets.Interface)]
    public class IRoutePrefixAttribute : Attribute
    {
        private readonly string prefix;

        public IRoutePrefixAttribute(string prefix)
        {
            this.prefix = prefix;
        }

        public string Prefix
        {
            get
            {
                return prefix;
            }
        }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InAttribute : Attribute
    {

        public InAttribute(InclutionTypes InclutionType)
        {
            this.InclutionType = InclutionType;
        }

        public InAttribute()
        {
            InclutionType = InclutionTypes.Path;
        }

        public InclutionTypes InclutionType { get; set; }
    }
}