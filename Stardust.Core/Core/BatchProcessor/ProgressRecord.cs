using System;

namespace Stardust.Core.BatchProcessor
{
    [Serializable]
    public class ProgressRecord
    {
        public string ItemId { get; set; }

        public string Action { get; set; }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public string StackTrace { get; set; }

        public DateTime EntryDateTime { get; set; }
        public string Message { get; set; }
    }
}