using System.Configuration;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class RepositoryFactory : IRepositoryFactory
    {

        public ConfigurationContext GetRepository()
        {
            return (ConfigurationContext) ContainerFactory.Current.Resolve(typeof(ConfigurationContext), Scope.Context, CreateRepository);
        }

        private static ConfigurationContext CreateRepository()
        {
            var connectionString = ConfigurationManager.AppSettings["configStore"];
            if (connectionString.IsNullOrWhiteSpace())
                connectionString ="Type=embedded;endpoint=http://localhost:8090/brightstar;StoresDirectory=C:\\Stardust\\Stores;StoreName=config";
            var context= new ConfigurationContext(connectionString);
            return context;
        }

        
    }
}