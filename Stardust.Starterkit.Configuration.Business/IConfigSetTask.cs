using System;
using System.Collections.Generic;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public interface IConfigSetTask : IDisposable
    {
        IConfigSet GetConfigSet(string name, string systemName);

        bool CreateConfigSet(string name, string systemName, IConfigSet parent);
        IServiceDescription CreateService(IConfigSet cs, string servicename);
        IEndpoint CreateEndpoint(IServiceDescription service, string endpointname, List<string> parameters=null);

        IEnvironment CreatEnvironment(IConfigSet cs, string environmentName);
        IEnumerable<IConfigSet> GetAllConfitSets();

        IConfigSet GetConfigSet(string id);

        string GenerateReaderKey(string id);
        IServiceDescription GetService(string id);
        IEndpoint GetEndpoint(string id);
        IEndpointParameter GetEnpointParameter(string id);
        void UpdateEndpointParameter(IEndpointParameter parameter);
        IEnvironment GetEnvironment(string id);
        ISubstitutionParameter GetSubstitutionParameter(string id);
        void UpdateSubstitutionParameter(ISubstitutionParameter parameter);
        void CreatEnvironmentParameter(IEnvironment env, string name, string itemValue, bool isSecureString);
        IEnvironmentParameter GetEnvironmentParameter(string id);
        void UpdateEnvironmentParameter(IEnvironmentParameter parameter);
        ConfigurationSet GetConfigSetData(string id, string environment);
        void CreateFromTextConfigStoreFile(ConfigurationSet configSet);
        void UpdateService(IServiceDescription service);
        IServiceHostSettings GetServiceHost(string id);
        IServiceHostSettings CreateServiceHost(IConfigSet configSet, string name);
        IServiceHostParameter CreateServiceHostParameter(IServiceHostSettings serviceHost, string name, bool isSecureString, string itemValue, bool isEnvironmental);
        IServiceHostParameter GetHostParameter(string id);
        void UpdateHostParameter(IServiceHostParameter serviceHostParameter);
        void UpdateAdministrators(ICollection<IConfigUser> administrators);
        void UpdateCacheSettingsParameter(ICacheSettings cacheType);

        void DeleteSubstitutionParameter(string envirionmentId, string parameterId);
        void CreateEndpointParameter(string item, string name, string itemValue, bool isSubstiturtionParameter);
        void DeleteEndpoint(IEndpoint endpoint);

        ConfigurationSettings InitializeDatacenter(string id, ConfigurationSettings settings);

        ConfigurationSettings PublishDatacenter(string id, ConfigurationSettings settings);

        void ConfigureEnvironment(string id, FederationSettings registration);

        void DeleteEnvironment(IEnvironment env);

        void UpdateServiceHost(IServiceHostSettings host);

        void UpdateEnvironment(IEnvironment environment);

        void UpdateConfigSet(IConfigSet configSet);

        void DeleteService(IServiceDescription service);

        void CreateSubstitutionParameter(IEnvironment env, string name, string value);

        string GenerateReaderKey(string id, string environment);

        bool PushChange(string id, string environment);

        string GenerateReaderKey(IConfigSet configSet);
    }
}