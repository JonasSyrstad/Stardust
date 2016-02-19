using System.Collections.Generic;
using System.ServiceModel;
using Stardust.Frontier.BatchProcessor;

namespace Stardust.Frontier.MasterData
{
    [ServiceContract]
    public interface IMasterDataManagementService
    {
        [OperationContract]
        string StartConnector(string name, ExecutionTypes executionType);

        [OperationContract]
        bool IsConnectorRunning(string name);

        [OperationContract]
        bool TerminateAll();

        [OperationContract]
        bool Terminate(string name);

        [OperationContract]
        IEnumerable<ProgressLogItem> GetLastEntries(int n, bool includeEmpty);

        [OperationContract]
        ProgressLogItem GetProgressHeader(string runId);

        [OperationContract]
        PagedList<ProgressRecord> GetRunDetails(string runId, int page, bool onlyErrors);

        [OperationContract]
        IDictionary<string, ConnectorTypes> GetConnectors();

    }

}
