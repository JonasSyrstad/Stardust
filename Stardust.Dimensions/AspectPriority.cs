using System;

namespace Stardust.Dimensions
{
    public enum AspectPriority
    {
        Low = 0,
        MediumLow = 1,
        Medium = 3,
        MediumHigh = 4,
        High = 5,
        /// <summary>
        /// Reserved for the framework.
        /// </summary>
        [Obsolete("Reserved for the TERS framework", false)]
        Highest = 6
    }
}