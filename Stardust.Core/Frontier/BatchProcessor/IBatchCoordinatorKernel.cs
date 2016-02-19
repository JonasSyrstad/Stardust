using System.Collections.Generic;

namespace Stardust.Frontier.BatchProcessor
{
    public interface IBatchCoordinatorKernel<T> : IBatchKernel<T>
    {
        IBatchCoordinatorKernel<T> Initialize(EnvironmentDefinition environment);

        IBatchConnector<T> GetConnector(string name);

        IEnumerable<string> GetConnectors(ConnectorTypes ofType);

        IDictionary<string, ConnectorTypes> GetConnectors();

        IBatchProgressLogger GetLogger();

        bool TerminateAllConnectors();
        
        bool TerminateConnector(string name);
    }
}