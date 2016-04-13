using System;
using System.Linq;
using Stardust.Core.Security;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class EnvironmentTasks : ConfigurationTaskBase, IEnvironmentTasks
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EnvironmentTasks(IRepositoryFactory factory)
            : base(factory)
        {
        }

        public void CreatEnvironmentParameter(IEnvironment env, string name, string itemValue, bool isSecureString)
        {
            var newPar = env.CreateParameters(Repository, name, isSecureString);
            newPar.SetValue(itemValue);
            env.AddToChildren(Repository, newPar);
            env.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            foreach (var environment in env.ConfigSet.Environments)
            {
                if (environment.EnvironmentParameters.Any(p => p.Name == name)) continue;
                var par = environment.CreateParameters(Repository, name, isSecureString);
                par.SetValue(itemValue);
                environment.AddToChildren(Repository, par);
                Repository.SaveChanges();
            }
        }

        public void UpdateEnvironmentParameter(IEnvironmentParameter parameter)
        {
            parameter.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
        }

        public IEnvironment CreatEnvironment(IConfigSet cs, string environmentName)
        {
            var env = cs.CreateEnvironment(Repository, environmentName);
            cs.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
            return env;
        }

        public IEnvironment GetEnvironment(string id)
        {
            try
            {
                var env = Repository.Environments.Single(x => x.Id == id);
                Logging.DebugMessage("Environment {0} loaded for {1}", env.Name, env.ConfigSet.Id);
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

        public void DeleteEnvironment(IEnvironment env)
        {
            Repository.DeleteObject(env);
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

        public void DeleteSubstitutionParameter(string envirionmentId, string parameterId)
        {
            var env = GetEnvironment(envirionmentId);
            var subParam = GetSubstitutionParameter(parameterId);
            env.SubstitutionParameters.Remove(subParam);
            Repository.DeleteObject(subParam);
            Repository.SaveChanges();
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

        public void ConfigureEnvironment(string id, FederationSettings registration)
        {
            var env = EnsureEnvironment(id, registration);
            EnsureSubstitutionParameters(registration, env);
            SetServiceHostAddresses(registration, env);
            UpdateOtherSettings(registration, env);
        }

        private IEnvironment EnsureEnvironment(string id, FederationSettings registration)
        {
            var configSet = GetConfigsetInternal(id);
            var env = configSet.Environments.SingleOrDefault(e => e.Name == registration.Environment);
            if (env.IsNull())
            {
                env = configSet.CreateEnvironment(Repository, registration.Environment);
                Repository.SaveChanges();
            }
            return env;
        }

        public string GenerateReaderKey(string id, string environment)
        {
            var env = GetEnvironment(id, environment);
            env.SetReaderKey(UniqueIdGenerator.CreateNewId(20).Encrypt(KeySalt));
            UpdateEnvironment(env);
            return env.GetReaderKey();
        }

        protected static EncryptionKeyContainer KeySalt
        {
            get
            {
                return new EncryptionKeyContainer("makeItHarderTowrite");
            }
        }

        private IEnvironment GetEnvironment(string id, string environment)
        {
            var configSet = GetConfigsetInternal(id);
            var env = configSet.Environments.Single(e => string.Equals(e.Name, environment, StringComparison.OrdinalIgnoreCase));
            return env;
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
        public void UpdateEnvironment(IEnvironment environment)
        {
            Repository.SaveChanges();
        }

        public bool PushChange(string id, string environment)
        {
            var env = GetEnvironment(id, environment);
            env.ETag = DateTimeOffset.UtcNow.Ticks.ToString();
            env.LastPublish = DateTime.UtcNow;
            UpdateEnvironment(env);
            return Resolver.Activate<ICacheManagementService>().TryUpdateCache(id, environment);
        }

        public void UpdateCacheSettingsParameter(ICacheSettings cacheType)
        {
            cacheType.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;
            Repository.SaveChanges();
        }
    }
}