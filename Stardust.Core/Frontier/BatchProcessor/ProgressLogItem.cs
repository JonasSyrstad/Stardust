using System;
using System.Collections.Generic;

namespace Stardust.Frontier.BatchProcessor
{
    [Serializable]
    public class ProgressLogItem
    {
        public bool Success { get; set; }
        public string RunId { get; set; }

        public string ConnectorName { get; set; }

        public List<ProgressRecord> Progress { get; set; }
        
        public List<ProgressRecord> Failed { get; set; }

        public DateTime LastEntry { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public int ItemsToProcess { get; set; }

        public int ProcessedItems { get; set; }

        public int FailedItems { get; set; }

        public string LastError { get; set; }

        public bool Completed { get; set; }

        public string ProgressMessage { get; set; }
        public string Environment { get; set; }
    }
}