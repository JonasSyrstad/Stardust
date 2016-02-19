using System.Collections.Generic;
using Stardust.Frontier.BatchProcessor;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{


    public class ImportFirstLastName : ConnectorBase<CoordinatedData>
    {
        private List<NameItem> Data;
        protected override void PrepareDataSet()
        {
            Data = new List<NameItem>
            {
                new NameItem {FirstName = "1", LastName = "Syrstad", Id = "1",},
                new NameItem {FirstName = "2", LastName = "Syrstad", Id = "2",},
                new NameItem {FirstName = "5", LastName = "Syrstad", Id = "5",},
                new NameItem {FirstName = "6", LastName = "Syrstad", Id = "6",},
                new NameItem {FirstName = "7", LastName = "Syrstad", Id = "7",},
                new NameItem {FirstName = "8", LastName = "Syrstad", Id = "8",},
                new NameItem {FirstName = "9", LastName = "Syrstad", Id = "9",},
                new NameItem {FirstName = "10", LastName = "Syrstad", Id = "10"}


            };
            ItemsToProcess = Data.Count;
        }

        protected override IBatchConnector<CoordinatedData> RunExecution(ExecutionTypes executionType)
        {
            foreach (var nameItem in Data)
            {
                CheckForTerminationSignal();
                Kernel.UpdateItem(nameItem.Id, new CoordinatedData { Id = nameItem.Id, FirstName = nameItem.FirstName, LastName = nameItem.LastName }, Name);
                UpdateProgress(nameItem.Id, "Import");
            }
            Data = null;
            return this;
        }

        public override string Name
        {
            get { return "ImportFirstLastName"; }
        }

        public override ConnectorTypes Type
        {
            get { return ConnectorTypes.Import; }
        }
    }
}