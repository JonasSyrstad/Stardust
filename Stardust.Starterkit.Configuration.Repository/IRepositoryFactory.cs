namespace Stardust.Starterkit.Configuration.Repository
{
    public interface IRepositoryFactory
    {
        ConfigurationContext GetRepository();
    }
}