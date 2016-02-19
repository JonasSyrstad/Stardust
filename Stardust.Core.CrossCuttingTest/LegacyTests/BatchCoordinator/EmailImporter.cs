using System.Collections.Generic;
using System.Threading;
using Stardust.Frontier.BatchProcessor;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    public class EmailImporter : ConnectorBase<CoordinatedData>
    {
        private List<EmailItem> Data;
        protected override void PrepareDataSet()
        {
            ItemsToProcess = 9;
            Data = new List<EmailItem>
            {
                new EmailItem{Id = "1", Email = "jsyrstad2+1@gmail.com"},
                new EmailItem{Id = "2", Email = "jsyrstad2+2@gmail.com"},
                new EmailItem{Id = "3", Email = "jsyrstad2+3@gmail.com"},
                new EmailItem{Id = "4", Email = "jsyrstad2+4@gmail.com"},
                new EmailItem{Id = "5", Email = "jsyrstad2+5@gmail.com"},
                new EmailItem{Id = "6", Email = "jsyrstad2+6@gmail.com"},
                new EmailItem{Id = "7", Email = "jsyrstad2+7@gmail.com"},
                new EmailItem{Id = "8", Email = "jsyrstad2+8@gmail.com"},
                new EmailItem{Id = "9", Email = "jsyrstad2+9@gmail.com"}
            };

        }

        protected override IBatchConnector<CoordinatedData> RunExecution(ExecutionTypes executionType)
        {
            Thread.Sleep(100);
            foreach (var emailItem in Data)
            {
                CheckForTerminationSignal();
                Kernel.UpdateItem(emailItem.Id, new CoordinatedData { Email = emailItem.Email, Id = emailItem.Id }, Name);
                UpdateProgress(emailItem.Id, "Import");
            }
            Data = null;
            return this;
        }

        public override string Name
        {
            get { return "EmailImporter"; }
        }


        public override ConnectorTypes Type
        {
            get { return ConnectorTypes.Import; }
        }
    }
}