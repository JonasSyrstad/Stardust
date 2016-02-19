using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Stardust.Frontier.BatchProcessor;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;
using Stardust.Particles;

namespace Stardust.Frontier.MasterData.Client
{
    /// <summary>
    /// A wrapper around the service, remember to place in an using statement 
    /// </summary>
    public class MasterDataClient : IMasterDataManagementService, IDisposable
    {
        private IServiceContainer<IMasterDataManagementService> Container;
        private ITracer Tracer;

        public MasterDataClient(string serviceName, BootstrapContext token = null)
        {
            Tracer = TracerFactory.StartTracer(this, "ctor");
            var runtime = RuntimeFactory.CreateRuntime();
            Container = runtime.CreateServiceProxy<IMasterDataManagementService>(serviceName);
            if (token.IsInstance())
                Container.Initialize(token);
        }

        ~MasterDataClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IDictionary<string, ConnectorTypes> GetConnectors()
        {
            using (TracerFactory.StartTracer(this,"GetConnectors"))
            {
                return Container.GetClient().GetConnectors(); 
            }
        }

        public IEnumerable<ProgressLogItem> GetLastEntries(int n, bool includeEmpty)
        {
            return Container.GetClient().GetLastEntries(n, includeEmpty);
        }

        public ProgressLogItem GetProgressHeader(string runId)
        {
            using (TracerFactory.StartTracer(this,string.Format("GetProgressHeader({0})", runId)))
            {
                return Container.GetClient().GetProgressHeader(runId); 
            }
        }

        public PagedList<ProgressRecord> GetRunDetails(string runId, int page, bool onlyErrors)
        {
            using (TracerFactory.StartTracer(this,string.Format("GetRunDetails({0},{1},{2})", runId,page,onlyErrors)))
            {
                return Container.GetClient().GetRunDetails(runId, page, onlyErrors); 
            }
        }

        public bool IsConnectorRunning(string name)
        {
            using (TracerFactory.StartTracer(this,string.Format("IsConnectorRunning({0})", name)))
            {
                return Container.GetClient().IsConnectorRunning(name); 
            }
        }

        public string StartConnector(string name, ExecutionTypes executionType)
        {
            using (TracerFactory.StartTracer(this,string.Format("StartConnector({0},{1})", name,executionType)))
            {
                return Container.GetClient().StartConnector(name, executionType); 
            }
        }
        public bool Terminate(string name)
        {
            using (TracerFactory.StartTracer(this,string.Format("Terminate({0})", name)))
            {
                return Container.GetClient().Terminate(name); 
            }
        }

        public bool TerminateAll()
        {
            using (TracerFactory.StartTracer(this,"TerminateAll"))
            {
                return Container.GetClient().TerminateAll(); 
            }
        }
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Container.Dispose();
            Container = null;
            Tracer.Dispose();
            Tracer = null;
        }
    }
}
