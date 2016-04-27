using System;

namespace Stardust.Interstellar.Rest
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InAttribute : Attribute
    {
        public InclutionTypes InclutionType { get; set; }
    }
}