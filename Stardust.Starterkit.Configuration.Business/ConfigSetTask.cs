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
        private ConfigurationContext Repository;
        public ConfigSetTask(IRepositoryFactory repositoryFactory, ICacheManagementService cacheController)
        {
            this.cacheController = cacheController;
            this.cacheController.Initialize(this);
            Repository = repositoryFactory.GetRepository();
            Repository.SavingChanges += SavingChanges;
        }

        private void SavingChanges(object sender, EventArgs e)
        {
            foreach (var brightstarEntityObject in from to in Repository.TrackedObjects where to.Implements<IEnvironment>() select to as IEnvironment)
            {
                var key = GetCacheKey(brightstarEntityObject.ConfigSet.Id, brightstarEntityObject.Name);
                ConfigurationSet old;
                ConfigSetCache.TryRemove(key, out old);
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
                UpdateCache(configSet);
            }

            return true;
        }

        public IEndpoint CreateEndpoint(IServiceDescription service, string endpointname, List<string> parameters = null)
        {
            var endpoint = service.CreateEndpoint(Repository, endpointname, false, parameters);
            endpoint.ServiceDescription.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(service.ConfigSet);
            return endpoint;
        }

        [Obsolete("This this causes update of all environments, should only update the last changed one")]
        private void UpdateCache(IConfigSet configSet)
        {
            //foreach (var env in configSet.Environments)
            //{

            //    Logging.DebugMessage("Updating cache for [{0}-{1}]", configSet.Name, env.Name);
            //    cacheController.TryUpdateCache(env.ConfigSet.Id, env.Name);
            //}

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

        public IEnvironment CreatEnvironment(IConfigSet cs, string environmentName)
        {
            var env = cs.CreateEnvironment(Repository, environmentName);
            cs.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            return env;
        }

        public void CreatEnvironmentParameter(IEnvironment env, string name, string itemValue, bool isSecureString)
        {
            var newPar = env.CreateParameters(Repository, name, isSecureString);
            newPar.SetValue(itemValue);
            env.AddToChildren(Repository, newPar);
            env.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(env.ConfigSet);
        }

        public IServiceDescription CreateService(IConfigSet cs, string servicename)
        {
            var service = cs.CreateService(Repository, servicename);
            service.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(service.ConfigSet);
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
                    UpdateEnvironment(env);
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

        public string GenerateReaderKey(string id, string environment)
        {
            var env = GetEnvironment(id, environment);
            env.SetReaderKey(UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt));
            UpdateEnvironment(env);
            return env.GetReaderKey();
        }

        private IEnvironment GetEnvironment(string id, string environment)
        {
            var configSets = from cs in Repository.ConfigSets where cs.Id == id select cs;
            var configSet = configSets.Single();
            var env = configSet.Environments.Single(e => String.Equals(e.Name, environment, StringComparison.OrdinalIgnoreCase));
            return env;
        }

        public bool PushChange(string id, string environment)
        {
            var env = GetEnvironment(id, environment);
            env.ETag = DateTimeOffset.UtcNow.Ticks.ToString();
            env.LastPublish = DateTime.UtcNow;
            UpdateEnvironment(env);
            return cacheController.TryUpdateCache(id, environment);
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
                    UpdateCache(ep.ServiceDescription.ConfigSet);
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

        public IEnvironment GetEnvironment(string id)
        {
            try
            {
                var env = Repository.Environments.Single(x => x.Id == id);
                Logging.DebugMessage("Environment {0} loaded for {1}",env.Name,env.ConfigSet.Id);
                foreach (var serviceHostSettings in env.ConfigSet.ServiceHosts)
                {
                    try
                    {
                        if (env.SubstitutionParameters.All(s => !String.Equals(s.Name, ServiceHostAddressSubstitutionName(serviceHostSettings), StringComparison.OrdinalIgnoreCase)))
                        {
                            env.CreateSubstitutionParameters(Repository, ServiceHostAddressSubstitutionName(serviceHostSettings));
                            Repository.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Log("Unable to create sub param");
                    }
                }
                return env;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw new ArgumentException("Could not find item with id: " + id, "id", ex);
            }
        }



        private static string ServiceHostAddressSubstitutionName(IServiceHostSettings serviceHostSettingse)
        {
            return string.Format("{0}_Address", serviceHostSettingse.Name);
        }

        public IEnvironmentParameter GetEnvironmentParameter(string id)
        {
            try
            {
                return Repository.EnvironmentParameters.Single(x => x.Id == id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Could not find item with id: " + id, "id", ex);
            }
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

        public ISubstitutionParameter GetSubstitutionParameter(string id)
        {
            try
            {
                return Repository.SubstitutionParameters.Single(x => x.Id == id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Could not find item with id: " + id, "id", ex);
            }
        }

        public void UpdateEndpointParameter(IEndpointParameter parameter)
        {
            parameter.Endpoint.ServiceDescription.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(parameter.Endpoint.ServiceDescription.ConfigSet);
        }

        public void UpdateEnvironmentParameter(IEnvironmentParameter parameter)
        {
            parameter.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(parameter.Environment.ConfigSet);
        }

        public void UpdateService(IServiceDescription service)
        {
            service.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(service.ConfigSet);
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
            UpdateCache(serviceHost.ConfigSet);
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
                    environment.CreateSubstitutionParameters(Repository, serviceHost.Name + "_" + param.Name);
                }
            }
            Repository.SaveChanges();
            UpdateCache(serviceHost.ConfigSet);
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
                        environment.CreateSubstitutionParameters(Repository, paramName);
                }
            }
            Repository.SaveChanges();
            UpdateCache(serviceHostParameter.ServiceHost.ConfigSet);
        }

        public void UpdateAdministrators(ICollection<IConfigUser> administrators)
        {
            Repository.SaveChanges();
        }

        public void UpdateCacheSettingsParameter(ICacheSettings cacheType)
        {
            cacheType.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            UpdateCache(cacheType.Environment.ConfigSet);
        }

        public void DeleteSubstitutionParameter(string envirionmentId, string parameterId)
        {
            var env = GetEnvironment(envirionmentId);
            var subParam = GetSubstitutionParameter(parameterId);
            env.SubstitutionParameters.Remove(subParam);
            Repository.DeleteObject(subParam);
            Repository.SaveChanges();
            UpdateCache(env.ConfigSet);
        }

        public void CreateEndpointParameter(string item, string name, string itemValue, bool isSubstiturtionParameter)
        {
            var endpoint = GetEndpoint(item);
            endpoint.AddParameter(Repository, name, itemValue, isSubstiturtionParameter);
            Repository.SaveChanges();
            UpdateCache(endpoint.ServiceDescription.ConfigSet);
        }

        public void DeleteEndpoint(IEndpoint endpoint)
        {
            var configSet = endpoint.ServiceDescription.ConfigSet;
            Repository.DeleteObject(endpoint);
            Repository.SaveChanges();
            UpdateCache(configSet);
        }

        public ConfigurationSettings InitializeDatacenter(string id, ConfigurationSettings settings)
        {
            var configSet = GetConfigSet(id);
            var env = GetEnvironment(configSet.Id + "-" + settings.Environment);
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
            var env = GetEnvironment(id + "-" + settings.Environment);
            if (settings.DatabaseKeyName.ContainsCharacters())
            {
                var database = GetEnvironmentParameter(env.Id + "-" + settings.DatabaseKeyName);
                database.SetValue(settings.DatabaseAccessKey);
                this.UpdateEnvironmentParameter(database);
            }
            if (settings.ReplicationBusKeyName.ContainsCharacters())
            {
                var replicationBus = GetEnvironmentParameter(env.Id + "-" + settings.ReplicationBusKeyName);
                replicationBus.SetValue(settings.ReplicationBusAccessKey);
                this.UpdateEnvironmentParameter(replicationBus);
            }
            if (settings.ServiceBusKeyName.ContainsCharacters())
            {
                var database = GetEnvironmentParameter(env.Id + "-" + settings.ServiceBusKeyName);
                database.SetValue(settings.ServiceBusAccessKey);
                this.UpdateEnvironmentParameter(database);
            }
            Repository.SaveChanges();
            return settings;
        }

        public void ConfigureEnvironment(string id, FederationSettings registration)
        {
            var env = EnsureEnvironment(id, registration);
            EnsureSubstitutionParameters(registration, env);
            SetServiceHostAddresses(registration, env);
            UpdateOtherSettings(registration, env);
            UpdateCache(env.ConfigSet);
        }

        public void DeleteEnvironment(IEnvironment env)
        {
            Repository.DeleteObject(env);
            Repository.SaveChanges();
        }

        public void UpdateServiceHost(IServiceHostSettings host)
        {
            Repository.SaveChanges();
        }

        public void UpdateEnvironment(IEnvironment environment)
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

        public void CreateSubstitutionParameter(IEnvironment env, string name, string value)
        {
            var item = Repository.SubstitutionParameters.Create();
            item.Environment = env;
            item.EnvironmentNameId = env.Id;
            item.Name = name;
            item.ItemValue = value;
            UpdateSubstitutionParameter(item);
        }

        private void UpdateOtherSettings(FederationSettings registration, IEnvironment env)
        {
            foreach (var serviceHostSettings in registration.OtherSettings)
            {
                var serviceHost = env.ConfigSet.ServiceHosts.SingleOrDefault(sh => sh.Name == serviceHostSettings.Key);
                foreach (var item in serviceHostSettings.Value)
                {
                    var shParam = serviceHost.Parameters.SingleOrDefault(p => p.Name == item.Key);
                    if (shParam.IsNull()) shParam = serviceHost.CreateParameter(Repository, item.Key, false, true);
                    if (shParam.Value != item.Value) shParam.ItemValue = "{0}";
                    var envParam = env.SubstitutionParameters.SingleOrDefault(p => p.Name == string.Format("{0}_{1}", serviceHost.Name, shParam.Name));
                    if (envParam.IsNull())
                        envParam = env.CreateSubstitutionParameters(Repository, string.Format("{0}_{1}", serviceHost.Name, shParam.Name));
                    if (envParam.Value != item.Value) envParam.ItemValue = item.Value;
                    Repository.SaveChanges();
                }
            }
        }

        private IEnvironment EnsureEnvironment(string id, FederationSettings registration)
        {
            var configSet = GetConfigSet(id);
            var env = configSet.Environments.SingleOrDefault(e => e.Name == registration.Environment);
            if (env.IsNull())
            {
                env = configSet.CreateEnvironment(Repository, registration.Environment);
                Repository.SaveChanges();
            }
            return env;
        }

        private void SetServiceHostAddresses(FederationSettings registration, IEnvironment env)
        {
            if (registration.ServiceHostRootUrl.IsNull())
            {
                return;
            }
            foreach (var sh in registration.ServiceHostRootUrl)
            {
                EnsureSubstitutionParameter(env, string.Format("{0}_Address", sh.Key), sh.Value);
            }
        }

        private void EnsureSubstitutionParameters(FederationSettings registration, IEnvironment env)
        {
            var val = registration.FederationServerUrl;
            EnsureAccountCredentials(registration, env);
            EnsureSubstitutionParameter(env, "IssuerActAsAddress", val);
            EnsureSubstitutionParameter(env, "IssuerAddress", val);
            EnsureSubstitutionParameter(env, "IssuerMetadataAddress", val);
            val = registration.FederationNamespace;
            EnsureSubstitutionParameter(env, "IssuerName", val);
            val = registration.PassiveFederationEndpoint;
            EnsureSubstitutionParameter(env, "StsAddress", val);
            val = registration.Thumbprint;
            EnsureSubstitutionParameter(env, "Thumbprint", val);
            val = registration.WebApplicationUrl;
            if (!val.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) val = "https://" + val;
            EnsureSubstitutionParameter(env, "Realm", val);
            EnsureSubstitutionParameter(env, "Audience", "{0}");
            EnsureSubstitutionParameter(env, "CertificateValidationMode", "None");
            EnsureSubstitutionParameter(env, "EnforceCertificateValidation", "false");
            EnsureSubstitutionParameter(env, "RequireHttps", "false");
        }

        private void EnsureAccountCredentials(FederationSettings registration, IEnvironment env)
        {
            SetAccountUserName(registration, env);
            SetAccountPassword(registration, env);
        }

        private void SetAccountPassword(FederationSettings registration, IEnvironment env)
        {
            EnsureEnvironmentParameter(env, "ServiceAccountPassword", registration.DelegationPassword, true);
        }

        private void EnsureEnvironmentParameter(IEnvironment env, string name, string value, bool secure)
        {
            var parameter = env.EnvironmentParameters.SingleOrDefault(p => p.Name == name);
            if (parameter.IsNull())
            {
                parameter = env.CreateParameters(Repository, name, secure);
            }
            if (secure)
            {
                parameter.SetValue(value);
            }
            else
            {
                if (parameter.Value != value)
                {
                    parameter.ItemValue = value;
                }
            }
            Repository.SaveChanges();
        }

        private void SetAccountUserName(FederationSettings registration, IEnvironment env)
        {
            EnsureEnvironmentParameter(env, "ServiceAccountName", registration.DelegationUserName, false);

        }

        private void EnsureSubstitutionParameter(IEnvironment env, string name, string val)
        {
            var param = env.SubstitutionParameters.SingleOrDefault(p => p.Name == name);
            if (param.IsNull())
            {
                param = env.CreateSubstitutionParameters(Repository, name);
            }
            if (!param.Value.Equals(val, StringComparison.InvariantCultureIgnoreCase))
                param.ItemValue = val;
            Repository.SaveChanges();
        }

        public void UpdateSubstitutionParameter(ISubstitutionParameter parameter)
        {
            parameter.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;

            Repository.SaveChanges();
            UpdateCache(parameter.Environment.ConfigSet);
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