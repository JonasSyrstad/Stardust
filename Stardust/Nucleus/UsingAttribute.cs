using System;

namespace Stardust.Nucleus
{
    /// <summary>
    /// Added to the constructor the IOC container should use when crating instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class UsingAttribute : Attribute
    {
    }
}