using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stardust.Core.Security;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus.Extensions;
using Stardust.Particles;
using Stardust.Particles.Collection;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class ConfigSetTask : IConfigSetTask
    {
        private readonly ICacheManagementService cacheController;

        private readonly IEnvironmentTasks environmentTasks;


        private ConfigurationContext Repository;
        public ConfigSetTask(IRepositoryFactory repositoryFactory, IEnvironmentTasks environmentTasks)
        {
            this.cacheController = cacheController;
            this.environmentTasks = environmentTasks;
            Repository = repositoryFactory.GetRepository();
            Repository.SavingChanges += SavingChanges;
        }

        private void SavingChanges(object sender, EventArgs e)
        {
            try
            {
                foreach (var brightstarEntityObject in from to in Repository.TrackedObjects where to.Implements<IEnvironment>() select to as IEnvironment)
                {
                    var key = GetCacheKey(brightstarEntityObject.ConfigSet.Id, brightstarEntityObject.Name);
                    ConfigurationSet old;
                    ConfigSetCache.TryRemove(key, out old);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        ~ConfigSetTask()
        {
            Dispose(false);
        }

        public bool CreateConfigSet(string name, string systemName, IConfigSet parent)
        {
            var configSets = from cs in Repository.ConfigSets where cs.Name == name && cs.System == systemName select cs;
            if (configSets.Count() != 0) throw new AmbiguousMatchException(string.Format("Config set '{0}\\{1}' already exists", name, systemName));
            if (parent.IsNull())
            {
                configSets = from cs in Repository.ConfigSets where cs.System == systemName select cs;
                if (configSets.Count() != 0) throw new AmbiguousMatchException(string.Format("Config set '{0}\\{1}'... system {1} already exists", name, systemName));
            }
            var configSet = Repository.ConfigSets.Create();
            if (parent.IsInstance())
            {
                foreach (var administrator in parent.Administrators)
                {
                    configSet.Administrators.Add(administrator);
                }
            }
            else
            {
                if (ConfigReaderFactory.CurrentUser != null)
                    configSet.Administrators.Add(ConfigReaderFactory.CurrentUser);
            }

            configSet.Name = name;
            configSet.Created = DateTime.UtcNow;
            configSet.System = systemName;
            configSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            if (parent.IsInstance())
            {

                configSet.ParentConfigSet = parent;
                foreach (var serviceDescription in parent.Services)
                {
                    var c = serviceDescription.CreateChild(Repository, ref configSet);
                    configSet.Services.Add(c);
                    Repository.SaveChanges();
                }
                foreach (var serviceHostSettingse in parent.ServiceHosts)
                {
                    var hostChild = serviceHostSettingse.CreateChild(Repository, ref configSet);
                    configSet.ServiceHosts.Add(hostChild);
                    Repository.SaveChanges();
                }
                foreach (var environment in parent.Environments)
                {
                    var child = environment.CreateChild(Repository, ref configSet);
                    configSet.Environments.Add(child);
                    Repository.SaveChanges();
                }
                Repository.SaveChanges();
            }

            return true;
        }

        public IEndpoint CreateEndpoint(IServiceDescription service, string endpointname, List<string> parameters = null)
        {
            var endpoint = service.CreateEndpoint(Repository, endpointname, false, parameters);
            endpoint.ServiceDescription.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            return endpoint;
        }


        public void CreateFromTextConfigStoreFile(ConfigurationSet configSet)
        {
            var newSet = Repository.ConfigSets.Create();
            newSet.Administrators.Add(ConfigReaderFactory.CurrentUser);
            newSet.Name = configSet.SetName;
            newSet.System = configSet.SetName;
            newSet.Created = DateTime.UtcNow;
            foreach (var e in configSet.Environments)
            {
                var env = newSet.CreateEnvironment(Repository, e.EnvironmentName, true);
                foreach (var p in e.Parameters)
                {
                    var param = env.CreateParameters(Repository, p.Name, p.BinaryValue.ContainsElements());
                    param.ItemValue = p.Value;
                    if (p.BinaryValue.ContainsElements())
                    {
                        param.BinaryValue = p.BinaryValue;
                        param.SetValue(Convert.ToBase64String(p.BinaryValue));
                    }
                }
                env.AddIdentitySettingsToEnvironment(Repository, (from s in configSet.Services where s.IdentitySettings != null select s.IdentitySettings).FirstOrDefault());
            }
            foreach (var s in configSet.Endpoints)
            {
                var service = newSet.CreateService(Repository, s.ServiceName);
                Repository.SaveChanges();
                foreach (var ep in s.Endpoints)
                {
                    var endpoint = service.CreateEndpoint(Repository, ep.BindingType, true);
                    endpoint.SetFromRawData(ep, Repository);
                }
                service.ClientEndpointValue = s.ActiveEndpoint;
            }
            if (configSet.Services.IsInstance())
            {
                foreach (var host in configSet.Services)
                {
                    try
                    {
                        var newHost = newSet.CreateServiceHost(Repository, host.ServiceName);
                        foreach (var configParameter in host.Parameters)
                        {
                            var param = newHost.CreateParameter(Repository, configParameter.Name, configParameter.BinaryValue.ContainsElements(), false);
                            param.ItemValue = configParameter.BinaryValue.ContainsElements()
                                    ? Convert.ToBase64String(configParameter.BinaryValue)
                                    : configParameter.Value;
                            param.BinaryValue = configParameter.BinaryValue;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new InvalidDataException(string.Format("Unable to create {0}: {1}", host.ServiceName, ex.Message), ex);
                    }
                }
            }
            newSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
        }





        public IServiceDescription CreateService(IConfigSet cs, string servicename)
        {
            var service = cs.CreateService(Repository, servicename);
            service.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            return service;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerable<IConfigSet> GetAllConfitSets()
        {
            if (ConfigReaderFactory.CurrentUser.AdministratorType != AdministratorTypes.SystemAdmin)
                return ConfigReaderFactory.CurrentUser.ConfigSet.ToList();
            var configSets = from cs in Repository.ConfigSets orderby cs.Id select cs;
            return configSets.ToList();
        }

        public IConfigSet GetConfigSet(string name, string systemName)
        {
            var configSet = GetConfigSetsWithName(name, systemName);
            return configSet.Single();
        }

        public IConfigSet GetConfigSet(string id)
        {
            var configSets = from cs in Repository.ConfigSets where cs.Id == id select cs;

            var configSet = configSets.Single();

            if (configSet.ReaderKey.IsNullOrWhiteSpace())
            {
                configSet.SetReaderKey(UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt));
                UpdateConfigSet(configSet);
            }
            foreach (var env in configSet.Environments)
            {
                if (env.ReaderKey.IsNullOrWhiteSpace())
                {
                    env.SetReaderKey(UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt));
                    environmentTasks.UpdateEnvironment(env);
                }
            }
            return configSet;
        }

        public string GenerateReaderKey(string id)
        {
            var configSets = from cs in Repository.ConfigSets where cs.Id == id select cs;

            var configSet = configSets.Single();
            configSet.ReaderKey = UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt);
            UpdateConfigSet(configSet);
            return configSet.ReaderKey;
        }

        public string GenerateReaderKey(IConfigSet configSet)
        {
            configSet.ReaderKey = UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt);
            return configSet.ReaderKey;
        }

        public List<string> GetAllConfigSetNames()
        {
            return Repository.ConfigSets.Select(c => c.Id).ToList();
        }

        public void DeleteConfigSet(IConfigSet cs)
        {
           Repository.DeleteObject(cs);
            Repository.SaveChanges();
        }

        private static EncryptionKeyContainer KeySalt
        {
            get
            {
                return new EncryptionKeyContainer("makeItHarderTowrite");
            }
        }

        public ConfigurationSet GetConfigSetData(string id, string environment)
        {

            ConfigurationSet set;
            if (!TryGetSetFromCache(id, environment.ToLowerInvariant(), out set))
            {
                bool doSave;
                var configSet = GetConfigSet(id);
                set = configSet.GetRawConfigData(environment, out doSave);
                if (doSave)
                    UpdateConfigSet(configSet);
                AddToCache(id, environment, set);
            }
            return set;
        }

        private void AddToCache(string id, string environment, ConfigurationSet set)
        {
            ConfigSetCache.AddOrUpdate(GetCacheKey(id, environment), set);
        }



        private static ConcurrentDictionary<string, ConfigurationSet> ConfigSetCache = new ConcurrentDictionary<string, ConfigurationSet>(new Dictionary<string, ConfigurationSet>());



        private bool TryGetSetFromCache(string id, string environment, out ConfigurationSet set)
        {
            ConfigurationSet item;
            if (!ConfigSetCache.TryGetValue(GetCacheKey(id, environment), out item))
            {
                set = null;
                return false;
            }
            if (item.LastUpdated <= GetConfigSet(id).LastUpdate)
            {
                set = null;
                return false;
            }
            set = item;
            return true;
        }

        private static string GetCacheKey(string id, string environment)
        {
            return string.Format("{0}[{1}]", id, environment);
        }

        public IEndpoint GetEndpoint(string id)
        {
            try
            {
                var ep = Repository.Endpoints.Single(x => x.Id == id);
                if (ep.Name == "custom")
                {
                    foreach (var endpointParameter in ep.Parameters)
                    {
                        endpointParameter.ConfigurableForEachEnvironment = true;
                        endpointParameter.IsPerService = true;
                    }
                    Repository.SaveChanges();
                }
                return ep;
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Could not find item with id: " + id, "id", ex);
            }
        }

        public IEndpointParameter GetEnpointParameter(string id)
        {
            return Repository.EndpointParameters.Single(x => x.Id == id);
        }









        public IServiceDescription GetService(string id)
        {
            try
            {
                return Repository.ServiceDescriptions.Single(x => x.Id == id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Could not find item with id: " + id, "id", ex);
            }
        }



        public void UpdateEndpointParameter(IEndpointParameter parameter)
        {
            parameter.Endpoint.ServiceDescription.ConfigSet.LastUpdate = DateTime.UtcNow;
            if (parameter.ConfigurableForEachEnvironment)
            {
                foreach (var environment in parameter.Endpoint.ServiceDescription.ConfigSet.Environments)
                {
                    var param = environment.SubstitutionParameters.SingleOrDefault(p=>p.Name== parameter.Endpoint.ServiceDescription.Name+"_"+parameter.Name);
                    if(param!=null&&!parameter.SubstitutionParameters.Contains(param))
                        parameter.SubstitutionParameters.Add(param);
                }
                if (parameter.Endpoint.ServiceDescription.ServiceHost != null)
                {
                    var hostParam=parameter.Endpoint.ServiceDescription.ServiceHost.Parameters.SingleOrDefault(p => p.Name == parameter.Name);
                    if (hostParam != null)
                    {
                        if(!parameter.HostParameters.Contains(hostParam))
                            parameter.HostParameters.Add(hostParam);
                    }
                }
            }
            Repository.SaveChanges();
        }



        public void UpdateService(IServiceDescription service)
        {
            service.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
        }

        public IServiceHostSettings GetServiceHost(string id)
        {
            return Repository.ServiceHostSettingss.Single(x => x.Id == id);
        }

        public IServiceHostSettings CreateServiceHost(IConfigSet configSet, string name)
        {
            var serviceHost = configSet.CreateServiceHost(Repository, name);
            configSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            return serviceHost;
        }

        public IServiceHostParameter CreateServiceHostParameter(IServiceHostSettings serviceHost, string name, bool isSecureString, string itemValue, bool isEnvironmental)
        {
            var param = serviceHost.CreateParameter(Repository, name, isSecureString, isEnvironmental);
            param.SetValue(itemValue);
            param.ServiceHost.ConfigSet.LastUpdate = DateTime.UtcNow;
            if (param.IsEnvironmental)
            {
                foreach (var environment in serviceHost.ConfigSet.Environments)
                {

                    var keyName = serviceHost.Name + "_" + param.Name;
                    var envParam = environment.SubstitutionParameters.SingleOrDefault(p => p.Name == keyName);
                    if (envParam == null)
                    {
                        envParam = environment.CreateSubstitutionParameters(Repository, keyName);
                    }
                    if(param.SubstitutionParameters==null)
                        param.SubstitutionParameters=new List<ISubstitutionParameter>();
                    if (!param.SubstitutionParameters.Contains(envParam))
                    {
                        param.SubstitutionParameters.Add(envParam);
                    }
                }
            }
            Repository.SaveChanges();
            return param;
        }

        public IServiceHostParameter GetHostParameter(string id)
        {
            return Repository.ServiceHostParameters.Single(x => x.Id == id);
        }

        public void UpdateHostParameter(IServiceHostParameter serviceHostParameter)
        {
            serviceHostParameter.ServiceHost.ConfigSet.LastUpdate = DateTime.UtcNow;
            if (serviceHostParameter.IsEnvironmental)
            {
                foreach (var environment in serviceHostParameter.ServiceHost.ConfigSet.Environments)
                {
                    var paramName = string.Format("{0}_{1}", serviceHostParameter.ServiceHost.Name, serviceHostParameter.Name);
                    var subParam = environment.SubstitutionParameters.SingleOrDefault(sp => sp.Name == paramName);
                    if (subParam.IsNull())
                        subParam=environment.CreateSubstitutionParameters(Repository, paramName);
                    if (serviceHostParameter.SubstitutionParameters == null)
                        serviceHostParameter.SubstitutionParameters = new List<ISubstitutionParameter>();
                    if (!serviceHostParameter.SubstitutionParameters.Contains(subParam))
                    {
                        serviceHostParameter.SubstitutionParameters.Add(subParam);
                    }
                }
            }
            Repository.SaveChanges();
        }

        public void UpdateAdministrators(ICollection<IConfigUser> administrators)
        {

            Repository.SaveChanges();

        }

        public void CreateEndpointParameter(string item, string name, string itemValue, bool isSubstiturtionParameter)
        {
            var endpoint = GetEndpoint(item);
            endpoint.AddParameter(Repository, name, itemValue, isSubstiturtionParameter);
            Repository.SaveChanges();
        }

        public void DeleteEndpoint(IEndpoint endpoint)
        {
            Repository.DeleteObject(endpoint);
            Repository.SaveChanges();
        }

        public ConfigurationSettings InitializeDatacenter(string id, ConfigurationSettings settings)
        {
            var configSet = GetConfigSet(id);
            var env = environmentTasks.GetEnvironment(configSet.Id + "-" + settings.Environment);
            env.CreateDataCenterKeyProperties(this.Repository, settings);
            Repository.SaveChanges();
            var subPar = env.EnsureDataCentersKey(Repository, settings);
            subPar.AddNewDataCenter(Repository, settings);
            env.ConfigSet.GetUris(Repository, settings);
            return settings;
        }

        public ConfigurationSettings PublishDatacenter(string id, ConfigurationSettings settings)
        {
            //var configSet = GetConfigSet(id);
            var env = environmentTasks.GetEnvironment(id + "-" + settings.Environment);
            if (settings.DatabaseKeyName.ContainsCharacters())
            {
                var database = environmentTasks.GetEnvironmentParameter(env.Id + "-" + settings.DatabaseKeyName);
                database.SetValue(settings.DatabaseAccessKey);
                environmentTasks.UpdateEnvironmentParameter(database);
            }
            if (settings.ReplicationBusKeyName.ContainsCharacters())
            {
                var replicationBus = environmentTasks.GetEnvironmentParameter(env.Id + "-" + settings.ReplicationBusKeyName);
                replicationBus.SetValue(settings.ReplicationBusAccessKey);
                environmentTasks.UpdateEnvironmentParameter(replicationBus);
            }
            if (settings.ServiceBusKeyName.ContainsCharacters())
            {
                var database = environmentTasks.GetEnvironmentParameter(env.Id + "-" + settings.ServiceBusKeyName);
                database.SetValue(settings.ServiceBusAccessKey);
                environmentTasks.UpdateEnvironmentParameter(database);
            }
            Repository.SaveChanges();
            return settings;
        }





        public void UpdateServiceHost(IServiceHostSettings host)
        {
            Repository.SaveChanges();
        }



        public void UpdateConfigSet(IConfigSet configSet)
        {
            Repository.SaveChanges();
        }

        public void DeleteService(IServiceDescription service)
        {
            Repository.DeleteObject(service);
            Repository.SaveChanges();
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            if (Repository != null) Repository.Dispose();
        }

        private IQueryable<IConfigSet> GetConfigSetsWithName(string name, string systemName)
        {
            var configSet = from cs in Repository.ConfigSets where cs.Name == name && cs.System == systemName select cs;
            return configSet;
        }
    }
}