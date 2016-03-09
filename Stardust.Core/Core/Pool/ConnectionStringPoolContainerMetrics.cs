using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Stardust.Core.Pool
{
    internal class ConnectionStringPoolContainerMetrics : IStardustPerformanceMetrics
    {
        private static TimeSpan sampleInterval;

        private static Timer timer;

        private static ConcurrentDictionary<string, MetricItem> metrics = new ConcurrentDictionary<string, MetricItem>();

        static Func<MetricItem, Task> metricsCollector;

        public IStardustPerformanceMetrics SetCollectionInterval(TimeSpan interval)
        {
            sampleInterval = interval;
            timer = new Timer(interval.TotalMilliseconds) { AutoReset = true, Enabled = true };
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            return this;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(metrics.Count==0) return;
            Task[] tasks = new Task[metrics.Count];
            var index = 0;
            foreach (var metricItem in metrics)
            {
                lock (metricItem.Value)
                {
                    tasks[index] = metricsCollector((MetricItem)metricItem.Value.Clone());
                    metricItem.Value.Average = 0;
                    metricItem.Value.Min = null;
                    metricItem.Value.Max = null;
                    metricItem.Value.Timestamp = DateTimeOffset.UtcNow;
                    metricItem.Value.Samples = 0;
                    metricItem.Value.Values.Clear();
                    index++;
                }
            }
            Task.Run(async () => await Task.WhenAll(tasks));
        }

        public void SetCallbackHandler(Func<MetricItem, Task> handler)
        {
            metricsCollector = handler;
        }

        internal ConnectionStringPoolContainerMetrics AddMetricsCollection(string name)
        {
            return AddMetricsCollection(name, null);
        }

        internal ConnectionStringPoolContainerMetrics AddMetricsCollection(string name, string description)
        {
            return AddMetricsCollection(name, description, null);
        }

        internal ConnectionStringPoolContainerMetrics AddMetricsCollection(string name, string description, double? avaliable)
        {
            metrics.TryAdd(name, new MetricItem { Name = name, Description = description, Available = avaliable });
            return this;
        }

        internal Task AddMetric(string name, double value)
        {
            if (timer == null || metricsCollector == null) return Task.FromResult(1);
            MetricItem item;
            if (!metrics.TryGetValue(name, out item)) return Task.FromResult(1);
            return Task.Run(() =>
                {
                    lock (item)
                    {
                        if (item.Values == null) item.Values = new List<double>();
                        item.Values.Add(value);
                    }
                });

        }
    }
}