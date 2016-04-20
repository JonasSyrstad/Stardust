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

        
        IEnumerable<IConfigSet> GetAllConfitSets();

        IConfigSet GetConfigSet(string id);

        string GenerateReaderKey(string id);
        IServiceDescription GetService(string id);
        IEndpoint GetEndpoint(string id);
        IEndpointParameter GetEnpointParameter(string id);
        void UpdateEndpointParameter(IEndpointParameter parameter);
        
        
        
        ConfigurationSet GetConfigSetData(string id, string environment);
        void CreateFromTextConfigStoreFile(ConfigurationSet configSet);
        void UpdateService(IServiceDescription service);
        IServiceHostSettings GetServiceHost(string id);
        IServiceHostSettings CreateServiceHost(IConfigSet configSet, string name);
        IServiceHostParameter CreateServiceHostParameter(IServiceHostSettings serviceHost, string name, bool isSecureString, string itemValue, bool isEnvironmental);
        IServiceHostParameter GetHostParameter(string id);
        void UpdateHostParameter(IServiceHostParameter serviceHostParameter);
        void UpdateAdministrators(ICollection<IConfigUser> administrators);
        

        
        void CreateEndpointParameter(string item, string name, string itemValue, bool isSubstiturtionParameter);
        void DeleteEndpoint(IEndpoint endpoint);

        ConfigurationSettings InitializeDatacenter(string id, ConfigurationSettings settings);

        ConfigurationSettings PublishDatacenter(string id, ConfigurationSettings settings);

        

        void UpdateServiceHost(IServiceHostSettings host);

        

        void UpdateConfigSet(IConfigSet configSet);

        void DeleteService(IServiceDescription service);

        


        string GenerateReaderKey(IConfigSet configSet);

        List<string> GetAllConfigSetNames();

        void DeleteConfigSet(IConfigSet cs);

        void DeleteServiceHost(IServiceHostSettings host);
    }
}