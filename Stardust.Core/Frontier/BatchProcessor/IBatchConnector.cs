using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stardust.Frontier.BatchProcessor
{
    public interface IBatchConnector<T>
    {
        IBatchConnector<T> Initialize(IBatchKernel<T> kernel, EnvironmentDefinition environment);
        IBatchConnector<T> SetLogger(IBatchProgressLogger logger);

        Task<IBatchConnector<T>> ExecuteConnector(ExecutionTypes executionType);

        string Name { get; }

        int ProcessedItems { get; }

        int FailedItems { get;}

        int ItemsToProcess { get; }

        bool IsRunning { get; }

        IEnumerable<ErrorItem> Errors { get; }

        ConnectorTypes Type { get; }

        string LastError { get;}

        string RunId { get; }
        void Terminate();
    }
}