using System;
using System.Collections.Generic;
using Stardust.Core.BatchProcessor;
using Stardust.Particles;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;

namespace Stardust.MasterData.Service
{
    public abstract class MasterDataManagementService<T> : IMasterDataManagementService where T : class
    {
        private IRuntime Runtime;
        private ITracer Tracer;

        protected virtual void Initialize(string methodName)
        {
            Runtime =
                RuntimeFactory.CreateRuntime()
                    .InitializeWithConfigSetName(ConfigurationManagerHelper.GetValueOnKey("sonfigSet"));
            Runtime.SetEnvironment(ConfigurationManagerHelper.GetValueOnKey("environment"));
            Tracer = Runtime.SetServiceName(this, ServiceName, methodName);

        }

        protected virtual void TearDown()
        {
            Tracer.Dispose();
        }

        protected abstract string ServiceName { get; }

        public string StartConnector(string name, ExecutionTypes executionType)
        {
            Initialize(string.Format("StartConnector({0})", name));
            IBatchConnector<T> task;
            try
            {
                task = BatchProcessorFactory.GetKernel<T>().GetConnector(name);
                task.ExecuteConnector(executionType);
            }
            catch (Exception ex)
            {
                Tracer.SetAdidtionalInformation(ex.Message);
                Tracer.SetErrorState(ex);
                throw;
            }
            TearDown();
            return task.RunId;
        }

        public bool IsConnectorRunning(string name)
        {
            Initialize(string.Format("IsConnectorRunning({0})", name));
            var result = BatchProcessorFactory.GetKernel<T>().IsSynchronizerRunning(name);
            TearDown();
            return result;
        }

        public bool TerminateAll()
        {
            Initialize("TerminateAll");
            bool result;
            try
            {
                result = BatchProcessorFactory.GetKernel<T>().TerminateAllConnectors();
            }
            catch (Exception ex)
            {
                Tracer.SetErrorState(ex);
                Tracer.SetAdidtionalInformation("Unable to terminate connectors");
                result = false;
            }
            TearDown();
            return result;

        }

        public bool Terminate(string name)
        {
            Initialize(string.Format("Terminate({0})", name));
            bool result;
            try
            {
                result = BatchProcessorFactory.GetKernel<T>().TerminateConnector(name);
            }
            catch (Exception ex)
            {
                Tracer.SetErrorState(ex);
                Tracer.SetAdidtionalInformation("Unable to terminate connectors");
                result = false;
            }
            TearDown();
            return result;
        }

        public IEnumerable<ProgressLogItem> GetLastEntries(int n, bool includeEmpty)
        {
            Initialize(string.Format("GetLastEntries({0},{1})", n, includeEmpty));
            var result = GetLogger().GetLastLogItems(n, includeEmpty);
            TearDown();
            return result;
        }

        private static IBatchProgressLogger GetLogger()
        {
            return BatchProcessorFactory.GetKernel<T>().GetLogger();
        }

        public ProgressLogItem GetProgressHeader(string runId)
        {
            Initialize(string.Format("GetProgressHeader({0})", runId));
            var result = GetLogger().GetLogItemHeader(runId);
            TearDown();
            return result;
        }

        public PagedList<ProgressRecord> GetRunDetails(string runId, int page, bool onlyErrors)
        {
            Initialize(string.Format("GetRunDetails({0},{1},{2})", runId, page, onlyErrors));
            var result = GetLogger().GetProcessRecords(runId, page, onlyErrors);
            TearDown();
            return result;
        }

        public IDictionary<string, ConnectorTypes> GetConnectors()
        {
            Initialize("GetConnectors");
            var result = BatchProcessorFactory.GetKernel<T>().GetConnectors();
            TearDown();
            return result;
        }
    }
}

