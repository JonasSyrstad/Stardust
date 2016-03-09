using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public interface IEnvironmentTasks
    {
        IEnvironment CreatEnvironment(IConfigSet cs, string environmentName);
        IEnvironment GetEnvironment(string id);
        void CreatEnvironmentParameter(IEnvironment env, string name, string itemValue, bool isSecureString);
        void UpdateEnvironmentParameter(IEnvironmentParameter parameter);
        void DeleteEnvironment(IEnvironment env);
        void UpdateEnvironment(IEnvironment environment);
        IEnvironmentParameter GetEnvironmentParameter(string id);
        void DeleteSubstitutionParameter(string envirionmentId, string parameterId);
        void CreateSubstitutionParameter(IEnvironment env, string name, string value);
        void ConfigureEnvironment(string id, FederationSettings registration);
        void UpdateSubstitutionParameter(ISubstitutionParameter parameter);
        ISubstitutionParameter GetSubstitutionParameter(string id);

        bool PushChange(string id, string environment);
        void UpdateCacheSettingsParameter(ICacheSettings cacheType);

        string GenerateReaderKey(string id, string name);
    }
}