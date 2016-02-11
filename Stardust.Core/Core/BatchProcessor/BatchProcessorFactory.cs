using Stardust.Nucleus;

namespace Stardust.Core.BatchProcessor
{
    public static class BatchProcessorFactory
    {
        public static IBatchCoordinatorKernel<T> GetKernel<T>()
        {
            var kernel = Resolver.Activate<IBatchCoordinatorKernel<T>>(InitializeKernel);
            return kernel;
        }

        private static void InitializeKernel<T>(IBatchCoordinatorKernel<T> k)
        {
            var environment = Resolver.Activate<IBatchEnviromentResolver>().GetEnvironment();
            k.Initialize(environment);
        }

        public static BatchConfigurator<TDataType> SetCoordinator<TDataType, TConnector>()
            where TConnector : IBatchConnector<TDataType>, new()
            where TDataType : class
        {
            return new BatchConfigurator<TDataType>().SetCoordinator<TConnector>();
        }

        public static BatchConfigurator<T> BindKernel<T>() where T : class
        {
            Resolver.GetConfigurator()  .Bind<IBatchEnviromentResolver>().To<BatchEnviromentResolver>().SetRequestResponseScope().DisableOverride();
            return new BatchConfigurator<T>().BindKernel();
        }

        public static BatchConfigurator<TDataType> SetBatchRepository<TDataType, T>()
            where T : IKernelRepository
            where TDataType : class
        {
            return new BatchConfigurator<TDataType>().SetBatchRepository<T>();
        }

        public static BatchConfigurator<TDataType> SetLogger<TDataType, T>()
            where TDataType : class
            where T : IBatchProgressLogger
        {
            return new BatchConfigurator<TDataType>().SetLogger<T>();
        }
    }
}