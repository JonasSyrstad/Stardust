using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Stardust.Interstellar;

namespace Stardust.Core.BatchProcessor
{
    public abstract class ConnectorBase<T> : IBatchConnector<T>
    {
        protected EnvironmentDefinition Environment;
        protected IBatchKernel<T> Kernel;
        protected IBatchProgressLogger Logger;
        private int ItemsProcesssed;
        protected bool IsTerminating { get; private set; }
        public int ProcessedItems { get; protected set; }
        public int FailedItems { get; protected set; }

        public int ItemsToProcess { get; protected set; }

        public bool IsRunning
        {
            get { return Kernel.IsSynchronizerRunning(Name); }
            private set { Kernel.SetSynchronizationRunState(value, Name); }
        }

        public IEnumerable<ErrorItem> Errors
        {
            get { return new List<ErrorItem>(); }
        }

        public abstract ConnectorTypes Type { get; }

        public string LastError { get; protected set; }


        public virtual IBatchConnector<T> Initialize(IBatchKernel<T> kernel, EnvironmentDefinition environment)
        {
            Kernel = kernel;
            Environment = environment;
            return this;
        }

        public virtual IBatchConnector<T> SetLogger(IBatchProgressLogger logger)
        {
            Logger = logger;
            return this;
        }

        public Task<IBatchConnector<T>> ExecuteConnector(ExecutionTypes executionType)
        {
            if (IsRunning) throw new InvalidAsynchronousStateException(string.Format("{0} is already running", Name));
            ResetRunDetails();
            CheckForTerminationSignal();
            PrepareDataSet();
            Logger.SetBatchSize(RunId, ItemsToProcess, string.Format("Execution batch, type: {0}", executionType));
            CheckForTerminationSignal();
            return RuntimeFactory.Run(() => InternalExecutor(executionType));
        }

        protected void CheckForTerminationSignal()
        {
            if (!IsTerminating) return;
            Logger.FinalizeExecution(RunId, "Terminated by user");
            IsRunning = false;
            IsTerminating = false;
            throw new InvalidAsynchronousStateException("Terminated By User");
        }

        private void ResetRunDetails()
        {
            IsTerminating = false;
            RunId = Guid.NewGuid().ToString();
            Logger.InitializeNewExecution(RunId, Name, "Starting");
            LastError = null;
            IsRunning = true;
            ProcessedItems = 0;
            FailedItems = 0;
        }

        public string RunId { get; private set; }

        public abstract string Name { get; }

        private IBatchConnector<T> InternalExecutor(ExecutionTypes executionType)
        {
            try
            {
                CheckForTerminationSignal();
                var result = RunExecution(executionType);
                IsRunning = false;
                IsTerminating = false;
                Logger.FinalizeExecution(RunId);
                return result;
            }
            catch (Exception)
            {
                IsRunning = false;
                IsTerminating = false;
                throw;
            }

        }
        protected abstract IBatchConnector<T> RunExecution(ExecutionTypes executionType);

        protected virtual void PrepareDataSet()
        { }

        protected virtual void LogRunError(string itemId, string attemptedAction, string message, Exception ex = null)
        {
            FailedItems++;
            ProcessedItems++;
            Logger.InsertErrorMessage(RunId, itemId, attemptedAction, message, ex);
        }

        protected virtual void UpdateProgress(string itemId,string action)
        {
            ProcessedItems++;
            Logger.UpdateProgress(RunId, itemId, action);
        }


        public void Terminate()
        {
            if (IsRunning)
                IsTerminating = true;
        }
    }
}