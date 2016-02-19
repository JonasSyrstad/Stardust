using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Frontier.BatchProcessor;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    public class Exporter :ConnectorBase<CoordinatedData>
    {
        private IEnumerable<string> Data;
        public static List<CoordinatedData> ExportedItems=new List<CoordinatedData>(); 

        public override ConnectorTypes Type
        {
            get {return ConnectorTypes.Export;}
        }

        protected override void PrepareDataSet()
        {
            Data = Kernel.GetModifiedItems(DateTime.UtcNow.AddDays(-1));
            ItemsToProcess = Data.Count();
            ProcessedItems = 0;
            ExportedItems.Clear();
        }

        protected override IBatchConnector<CoordinatedData> RunExecution(ExecutionTypes executionType)
        {
            foreach (var itemId in Data)
            {
                CheckForTerminationSignal();
                var item = Kernel.GetItem(itemId);
                ExportedItems.Add((CoordinatedData)item.MasterItem);
                UpdateProgress(itemId, "Export");
            }
            Data = null;
            return this;
        }

        public override string Name
        {
            get { return "Exporter"; }
        }
    }
}