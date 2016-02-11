using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.BatchProcessor;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.BatchCoordinator
{
    [TestClass]
    public class BatchCoordinatorTests
    {
        private readonly Mutex testMutex = new Mutex(true, "MySpecificTestScenarioUniqueMutexString");

        [TestInitialize]
        public void Initialize()
        {
            ContainerFactory.Current.KillAllInstances();
            KernelScope=Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();
            BatchProcessorFactory.BindKernel<CoordinatedData>()
                .SetBatchRepository<MemoryRepository>()
                .SetLogger<LoggerMock>()
                .SetCoordinator<ImportFirstLastName>()
                .SetCoordinator<EmailImporter>()
                .SetCoordinator<Synchronizer>()
                .SetCoordinator<Exporter>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }
        /// <summary>
        /// Used in lock to ensure sequential processing of the tests
        /// If run in parallel the tests become undeterministic due to the restriction of only one execution of a given connector
        /// </summary>
        private static object TrioWing = new object();

        private IKernelContext KernelScope;

        [TestMethod]
        [TestCategory("Batch Coordinator")]
        public void ImportSyncAndExportFromTwoSourcesTest()
        {
            lock (TrioWing)
            {
                LoggerMock.LogRepository.Clear();
                var kernel = BatchProcessorFactory.GetKernel<CoordinatedData>();
                WaitForCompleation(kernel);
                var task = kernel.GetConnector("EmailImporter").ExecuteConnector(ExecutionTypes.Full);
                var task1 = kernel.GetConnector("ImportFirstLastName").ExecuteConnector(ExecutionTypes.Full);
                Task.WaitAll(task, task1);
                Assert.AreNotEqual(task.Result.ProcessedItems, task1.Result.ProcessedItems);
                var task2 = kernel.GetConnector("Synchronizer").ExecuteConnector(ExecutionTypes.Full);
                task2.Wait();
                var task3 = kernel.GetConnector("Exporter").ExecuteConnector(ExecutionTypes.Full);
                task3.Wait();
                Assert.AreEqual(10, MemoryRepository.Repository.Count);
                Assert.IsNotNull(MemoryRepository.Repository.First().Value.MasterItem);
                Assert.AreEqual(10, Exporter.ExportedItems.Count);
                Assert.AreEqual(10, LoggerMock.LogRepository[task3.Result.RunId].ProcessedItems);
                Assert.AreEqual(10, LoggerMock.LogRepository[task3.Result.RunId].Progress.Count);
                Assert.AreEqual(4, kernel.GetLogger().GetLastLogItems(100, false).Count());
                Assert.AreEqual(task3.Result.ProcessedItems, kernel.GetLogger().GetLogItemHeader(task3.Result.RunId).ProcessedItems);
                Assert.AreEqual(10, kernel.GetLogger().GetProcessRecords(task3.Result.RunId, 0, false).Items.Count());
                Assert.AreEqual(0, kernel.GetLogger().GetProcessRecords(task3.Result.RunId, 0, true).Items.Count());
            }

        }

        private static void WaitForCompleation(IBatchCoordinatorKernel<CoordinatedData> kernel)
        {
            while (kernel.GetConnector("EmailImporter").IsRunning)
            {
                Thread.Sleep(10);
            }
        }

        [TestMethod]
        [TestCategory("Batch Coordinator")]

        public void ImportStartTwoImportersFailureTest()
        {

            lock (TrioWing)
            {
                IBatchConnector<CoordinatedData> emailConnector1;
                IBatchConnector<CoordinatedData> emailConnector2;
                Task<IBatchConnector<CoordinatedData>> task = null;
                try
                {
                    
                    var kernel = BatchProcessorFactory.GetKernel<CoordinatedData>();
                    emailConnector1 = kernel.GetConnector("EmailImporter");
                    task = emailConnector1.ExecuteConnector(ExecutionTypes.Full);
                    emailConnector2 = kernel.GetConnector("EmailImporter");
                    var task1 = emailConnector2.ExecuteConnector(ExecutionTypes.Full);
                    Task.WaitAll(task, task1);
                    Assert.Fail("Should throw InvalidAsynchronousStateException");
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(InvalidAsynchronousStateException));
                }
            }
        }

        [TestMethod]
        [TestCategory("Batch Coordinator")]
        public void ImportStartAndTerminate()
        {

            lock (TrioWing)
            {
                var kernel = BatchProcessorFactory.GetKernel<CoordinatedData>();
                IBatchConnector<CoordinatedData> emailConnector = null;
                try
                {
                    emailConnector = kernel.GetConnector("EmailImporter");
                    var task = emailConnector.ExecuteConnector(ExecutionTypes.Full);
                    Thread.Sleep(50);
                    kernel.TerminateAllConnectors();
                    Task.WaitAll(task);
                    Assert.IsFalse(kernel.GetLogger().GetLogItemHeader(task.Result.RunId).Success);
                }
                catch (AggregateException ex)
                {
                    Assert.IsFalse(kernel.GetLogger().GetLogItemHeader(emailConnector.RunId).Success);
                    Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidAsynchronousStateException));
                }
                catch (Exception ex)
                {
                    Assert.IsFalse(kernel.GetLogger().GetLogItemHeader(emailConnector.RunId).Success);
                    Assert.IsInstanceOfType(ex, typeof(InvalidAsynchronousStateException));
                }
            }
        }

        
    }
}