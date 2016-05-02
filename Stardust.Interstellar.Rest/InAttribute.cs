using System;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InAttribute : Attribute
    {
        private InclutionTypes InclutionType1;

        public InAttribute(InclutionTypes InclutionType)
        {
            InclutionType1 = InclutionType;
        }

        public InAttribute()
        {
            InclutionType = InclutionTypes.Path;
        }

        public InclutionTypes InclutionType { get; set; }
    }
}