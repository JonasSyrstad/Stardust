using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stardust.Core.BatchProcessor;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    public class LoggerMock : IBatchProgressLogger
    {
        internal static Dictionary<string, ProgressLogItem> LogRepository = new Dictionary<string, ProgressLogItem>();
        private string Environment;

        public void InitializeNewExecution(string runId, string name, string message)
        {
            if (LogRepository.ContainsKey(runId)) throw new InvalidDataException("Log entry already exists");
            var logItem = new ProgressLogItem
            {
                RunId = runId,
                ConnectorName = name,
                StartDateTime = DateTime.UtcNow,
                LastEntry = DateTime.UtcNow,
                ProcessedItems = 0,
                FailedItems = 0,
                Failed = new List<ProgressRecord>(),
                Progress = new List<ProgressRecord>(),
                ProgressMessage = message,
                Environment = Environment
            };
            LogRepository.Add(runId, logItem);
        }


        public void SetBatchSize(string runId, int ItemsToProcess, string message)
        {
            var item = GetProgressLogItem(runId);
            item.ItemsToProcess = ItemsToProcess;
            item.ProgressMessage = message;
            item.LastEntry = DateTime.UtcNow;
        }

        private static ProgressLogItem GetProgressLogItem(string RunId)
        {
            ProgressLogItem item;
            if (!LogRepository.TryGetValue(RunId, out item)) throw new IndexOutOfRangeException("Unable to get log item");
            return item;
        }

        public void InsertErrorMessage(string runId, string itemId, string attemptedAction, string message, Exception exception)
        {
            var item = GetProgressLogItem(runId);
            item.FailedItems++;
            item.ProcessedItems++;
            var record = new ProgressRecord
            {
                Action = attemptedAction,
                ItemId = itemId,
                EntryDateTime = DateTime.UtcNow,
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Message = message,
                Success = false
            };
            item.Failed.Add(record);
            item.Progress.Add(record);
            item.LastEntry = DateTime.UtcNow;
            item.LastError = exception.Message;
        }

        public void UpdateProgress(string runId, string itemId, string action)
        {
            var item = GetProgressLogItem(runId);
            item.ProcessedItems++;
            item.Progress.Add(new ProgressRecord
            {
                Action = action,
                ItemId = itemId,
                EntryDateTime = DateTime.UtcNow,
                Message = "Success",
                Success = false
            });
            item.LastEntry = DateTime.UtcNow;
        }

        public void FinalizeExecution(string runId, string errorMessage = null)
        {
            var item = GetProgressLogItem(runId);
            item.EndDateTime = DateTime.UtcNow;
            item.Completed = true;
            if (errorMessage.ContainsCharacters())
            {
                item.LastError = errorMessage;
                item.ProgressMessage = "Completed with errors";
                item.Success = false;
            }
            else
            {
                if (item.FailedItems != 0)
                {
                    item.Success = false;
                    item.ProgressMessage = "Completed with errors";
                }
                else
                {
                    item.Success = true;
                    item.ProgressMessage = "Completed";
                }
            }
        }

        public IBatchProgressLogger SetEnvironment(EnvironmentDefinition environment)
        {
            Environment = environment.Name;
            return this;
        }

        public ProgressLogItem GetLogItemHeader(string runId)
        {
            var item = GetProgressLogItem(runId);
            return item;
        }

        public IEnumerable<ProgressLogItem> GetLastLogItems(int itemsToReturn, bool includeEmptyRuns)
        {
            var items = from i in LogRepository.Values select i;
            if (!includeEmptyRuns)
                items = from i in items where !(i.Success && i.FailedItems != 0) select i;
            var sorted = from i in items orderby i.StartDateTime descending select i;
            return sorted.Take(100);
        }

        public PagedList<ProgressRecord> GetProcessRecords(string forRunId, int page, bool onlyErrors)
        {
            var item = GetProgressLogItem(forRunId);
            var items = onlyErrors ? item.Failed : item.Progress;
            var totalPages = items.GetTotalPageCount(25);
            return new PagedList<ProgressRecord>
            {
                Page = page,
                Items = items.Skip(page*25),
                TotalPages = totalPages,
                PageSize = 25
            };
        }

    }
}