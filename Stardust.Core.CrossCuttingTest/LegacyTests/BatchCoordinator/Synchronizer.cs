using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Core.BatchProcessor;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    public class Synchronizer : ConnectorBase<CoordinatedData>
    {
        private IEnumerable<string> _ItemsToProsess;

        protected override void PrepareDataSet()
        {
            _ItemsToProsess = Kernel.GetModifiedItems(DateTime.UtcNow.AddDays(-1));
            ItemsToProcess = _ItemsToProsess.Count();
        }

        protected override IBatchConnector<CoordinatedData> RunExecution(ExecutionTypes executionType)
        {
            
            foreach (var itemId in _ItemsToProsess)
            {
                CheckForTerminationSignal();
                var coordinationItem = Kernel.GetItem(itemId);
                var master = coordinationItem.MasterItem as CoordinatedData;
                if (master.IsNull())
                {
                    master = new CoordinatedData { Id = itemId };
                    coordinationItem.MasterItem = master;
                }
                foreach (var source in coordinationItem.Sources)
                {
                    if (source.Key == "EmailImporter")
                    {
                        var data = source.Value as CoordinatedData;
                        if (data.IsInstance())
                            master.Email = data.Email;
                    }
                    else
                    {
                        var data = source.Value as CoordinatedData;
                        if (data.IsInstance())
                        {
                            master.FirstName = data.FirstName;
                            master.LastName = data.LastName;
                        }
                    }
                }
                Kernel.SetExportItem(itemId, coordinationItem);
                UpdateProgress(itemId,"SynchronizeMaster");
            }
            _ItemsToProsess = null;
            return this;
        }

        public override string Name
        {
            get { return "Synchronizer"; }
        }

        public override ConnectorTypes Type
        {
            get { return ConnectorTypes.Import; }
        }
    }
}