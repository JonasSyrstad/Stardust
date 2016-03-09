using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Core
{
    /// <summary>
    /// Hooks for reading performance metrics 
    /// </summary>
    public interface IStardustPerformanceMetrics
    {
        IStardustPerformanceMetrics SetCollectionInterval(TimeSpan interval);

        void SetCallbackHandler(Func<MetricItem, Task> handler);
    }

    public class MetricItem : ICloneable
    {
        /// <summary>
        /// the begining of the interval.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        public TimeSpan Interval { get; set; }

        public double? Max { get; set; }

        public int Samples { get; set; }

        public double? Min { get; set; }

        public double Average { get; set; }

        public double? Available { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<double> Values { get; set; } 

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new MetricItem
                       {
                           Available = Available,
                           Average = Values.Average(),
                           Name = Name,
                           Description = Description,
                           Interval = Interval,
                           Max = Values.Max(),
                           Min = Values.Min(),
                           Samples = Samples,
                           Timestamp = Timestamp,
                           Values = Values.ToArray().ToList()
                       };
        }
    }
}
