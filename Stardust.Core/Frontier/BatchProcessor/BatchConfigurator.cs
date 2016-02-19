using Stardust.Frontier.BatchProcessor.Default;
using Stardust.Nucleus;

namespace Stardust.Frontier.BatchProcessor
{
    public sealed class BatchConfigurator<T> where T : class
    {
        internal BatchConfigurator()
        {
            
        }

        public BatchConfigurator<T> SetBatchRepository<TRepository>() where TRepository : IKernelRepository
        {
            Resolver.GetConfigurator().Bind<IKernelRepository>().To<TRepository>();
            return this;
        }

        public BatchConfigurator<T> SetCoordinator< TConnector>() where TConnector : IBatchConnector<T>, new()
        {
            Resolver.GetConfigurator().Bind<IBatchConnector<T>>().To<TConnector>(new TConnector().Name).SetTransientScope();
            return this;
        }

        public BatchConfigurator<T> BindKernel()
        {
            Resolver.GetConfigurator().Bind<IBatchCoordinatorKernel<T>>().To<DefaultKernel<T>>();
            return this;
        }

        public BatchConfigurator<T> SetLogger<TLogger>() where TLogger : IBatchProgressLogger
        {
            Resolver.GetConfigurator().Bind<IBatchProgressLogger>().To<TLogger>().SetSingletonScope();
            return this;
        }
    }
}