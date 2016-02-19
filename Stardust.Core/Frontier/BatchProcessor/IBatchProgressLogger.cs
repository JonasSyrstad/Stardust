using System;
using System.Collections.Generic;

namespace Stardust.Frontier.BatchProcessor
{
    public interface IBatchProgressLogger
    {
        void InitializeNewExecution(string runId, string name, string starting);

        void SetBatchSize(string runId, int ItemsToProcess, string message);

        void InsertErrorMessage(string runId, string itemId, string attemptedAction, string message, Exception exception);

        void UpdateProgress(string runId, string itemId, string action);

        void FinalizeExecution(string runId, string errorMessage=null);


        IBatchProgressLogger SetEnvironment(EnvironmentDefinition Environment);


        ProgressLogItem GetLogItemHeader(string runId);


        IEnumerable<ProgressLogItem> GetLastLogItems(int itemsToReturn, bool includeEmptyRuns);


        PagedList<ProgressRecord> GetProcessRecords(string forRunId, int page, bool onlyErrors);
    }
}