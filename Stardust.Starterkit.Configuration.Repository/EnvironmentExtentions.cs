using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public static class CreateEnvironmentExtentions
    {
        internal static readonly string[] ServiceParameters =
        {
            "Thumbprint","Address","Audience","IssuerAddress","StsAddress","IssuerMetadataAddress","IssuerActAsAddress","IssuerName","CertificateValidationMode","RequireHttps","EnforceCertificateValidation","Realm"};

        internal static readonly string[] PrServiceParameters = { "Address", "Key", "UserName", "ContainerName", "SenderName", "Port", "KeepAlive", "Metadata" };

        public static IEnvironment AddIdentitySettingsToEnvironment(this IEnvironment environment, ConfigurationContext context, IdentitySettings identitySettings)
        {
            if (identitySettings.IsNull()) return environment;
            context.SaveChanges();
            SetIdeneityValue(environment, "StsAddress", identitySettings.IssuerAddress);
            SetIdeneityValue(environment, "IssuerName", identitySettings.IssuerName);
            SetIdeneityValue(environment, "CertificateValidationMode", identitySettings.CertificateValidationMode);
            SetIdeneityValue(environment, "Thumbprint", identitySettings.Thumbprint);
            SetIdeneityValue(environment, "Audience", identitySettings.Audiences.FirstOrDefault());
            SetIdeneityValue(environment, "Realm", identitySettings.Realm);
            SetIdeneityValue(environment, "RequireHttps", identitySettings.RequireHttps.ToString().ToLower());
            SetIdeneityValue(environment, "EnforceCertificateValidation", identitySettings.EnforceCertificateValidation.ToString().ToLower());
            return environment;
        }

        private static void SetIdeneityValue(IEnvironment environment, string name, string value)
        {
            try
            {
                var param = environment.SubstitutionParameters.SingleOrDefault(x => x.Name == name);
                param.ItemValue = value;
            }
            catch (Exception)
            {
                throw new IndexOutOfRangeException(string.Format("Unable to find parameter {0}", name));
            }
        }

        public static IEnvironment CreateEnvironment(this IConfigSet configSet, ConfigurationContext context, string name, bool isImport = false)
        {
            var environment = context.Environments.Create();
            environment.ConfigSet = configSet;
            environment.ConfigSetNameId = configSet.Id;
            environment.Name = name;
            foreach (var serviceParameter in ServiceParameters)
            {
                environment.CreateSubstitutionParameters(context, serviceParameter, isImport);
            }
            context.SaveChanges();
            foreach (var service in configSet.Services)
            {
                var addressOverride = string.Format("{0}_Address", service.Name);
                var overrideProp = environment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                {
                    var subParam = environment.CreateSubstitutionParameters(context, addressOverride);
                    if (isImport)
                        subParam.ItemValue = "{0}";
                }
            }
            configSet.Environments.Add(environment);
            context.SaveChanges();
            AddToChildren(configSet, context, environment);
            return environment;
        }

        private static void AddToChildren(IConfigSet configSet, ConfigurationContext context, IEnvironment environment)
        {
            foreach (var childConfigSet in configSet.ChildConfigSets)
            {
                var c = childConfigSet;
                var child = environment.CreateChild(context, ref c);
                context.SaveChanges();
                AddToChildren(c, context, child);
            }
        }

        public static ISubstitutionParameter CreateSubstitutionParameters(this IEnvironment environment, ConfigurationContext context, string name, bool isImport = false)
        {
            var substitutionParameter = context.SubstitutionParameters.Create();
            substitutionParameter.EnvironmentNameId = environment.Id;
            substitutionParameter.Environment = environment;
            substitutionParameter.Name = name;
            if (name.EndsWith("_Address"))
            {
                var addressRoot = environment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, "Address", StringComparison.OrdinalIgnoreCase));
                if (addressRoot.IsInstance() && substitutionParameter.IsRoot)
                    substitutionParameter.Parent = addressRoot;
            }
            if ((name == "Address" || name == "Audience") && isImport)
                substitutionParameter.ItemValue = "{0}";
            environment.SubstitutionParameters.Add(substitutionParameter);
            return substitutionParameter;
        }

        public static IEnvironmentParameter CreateParameters(this IEnvironment environment, ConfigurationContext context, string name, bool isSecureString)
        {
            var substitutionParameter = context.EnvironmentParameters.Create();
            substitutionParameter.EnvironmentNameId = environment.Id;
            substitutionParameter.Environment = environment;
            substitutionParameter.Name = name;
            substitutionParameter.IsSecureString = isSecureString;
            return substitutionParameter;
        }

        public static void AddNewDataCenter(this ISubstitutionParameter envParam, ConfigurationContext context, ConfigurationSettings settings)
        {
            List<string> datacenters = new List<string>();
            if (envParam.IsNull())
            {
                throw new InvalidDataException("new parameter not found???");
            }
            datacenters = envParam.ItemValue.ContainsCharacters()?envParam.ItemValue.Split('|').ToList():new List<string>();
            if(!datacenters.Contains(settings.DataCenterName))
                datacenters.Add(settings.DataCenterName);
            envParam.ItemValue = string.Join("|", datacenters);
            context.SaveChanges();
            settings.DataCenterList = datacenters;

            Logging.DebugMessage("Added new datacenter {0}", string.Join("|", datacenters));
        }

        public static ISubstitutionParameter EnsureDataCentersKey(this IEnvironment env, ConfigurationContext context, ConfigurationSettings settings)
        {
            var serviceHost = env.ConfigSet.ServiceHosts.SingleOrDefault(sh => sh.Name == settings.DataCenterServiceHostName);
            var datacentersKey = serviceHost.Parameters.SingleOrDefault(shp => string.Equals(shp.Name, "datacenters", StringComparison.OrdinalIgnoreCase));
            if (datacentersKey.IsNull())
            {
                datacentersKey = serviceHost.CreateParameter(context, "datacenters", false, true);
                datacentersKey.ItemValue = "{0}";
                if (datacentersKey.IsEnvironmental)
                {
                    foreach (var environment in serviceHost.ConfigSet.Environments)
                    {
                        environment.CreateSubstitutionParameters(context, serviceHost.Name + "_" + datacentersKey.Name);
                    }
                }
            }
            
            var envParam =
                env.SubstitutionParameters.SingleOrDefault(p => p.Name == GetDataCentersEnvironmentKey(datacentersKey));
            if (envParam.IsNull()) envParam = env.CreateSubstitutionParameters(context,GetDataCentersEnvironmentKey(datacentersKey));
            Logging.DebugMessage("created datacenter keys for {0} in {1}",settings.DataCenterServiceHostName,settings.Environment);
            return envParam;
        }

        public static void GetUris(this IConfigSet configSet, ConfigurationContext context, ConfigurationSettings settings)
        {
            settings.ReplicationBusUri = string.Format(
                configSet.ServiceHosts.Single(s => s.Name == settings.DataCenterServiceHostName)
                    .GetRawConfigData(settings.Environment)
                    .GetConfigParameter(settings.ReplicationUriBusKeyName), settings.DataCenterName, "<KEY>");

            settings.ServiceBusUri = string.Format(
                configSet.ServiceHosts.Single(s => s.Name == settings.DataCenterServiceHostName)
                    .GetRawConfigData(settings.Environment)
                    .GetConfigParameter(settings.ServiceBusUriKeyName), settings.DataCenterName,"<KEY>");

            settings.DatabaseUri = string.Format(
                configSet.ServiceHosts.Single(s => s.Name == settings.DataCenterServiceHostName)
                    .GetRawConfigData(settings.Environment)
                    .GetConfigParameter(settings.DatabaseUriKeyName), settings.DataCenterName, "<KEY>");
        }

        private static string GetDataCentersEnvironmentKey(IServiceHostParameter datacentersKey)
        {
            return datacentersKey.ServiceHost.Name + "_" + datacentersKey.Name;
        }


        public static void CreateDataCenterKeyProperties(this IEnvironment env, ConfigurationContext context, ConfigurationSettings settings)
        {
            if (settings.ReplicationBusKeyName.ContainsCharacters())
            {
                if(env.EnvironmentParameters.All(s => s.Name != settings.ReplicationBusKeyName))
                {
                    env.CreateParameters(context, settings.ReplicationBusKeyName, true);
                    context.SaveChanges();
                }
            }
            if (settings.ServiceBusKeyName.ContainsCharacters())
            {
                if (env.EnvironmentParameters.All(s => s.Name != settings.ServiceBusKeyName))
                {
                    env.CreateParameters(context, settings.ServiceBusKeyName, true);
                    context.SaveChanges();
                }
            }
            if (settings.DatabaseKeyName.ContainsCharacters())
            {
                if (env.EnvironmentParameters.All(s => s.Name != settings.DatabaseKeyName))
                {
                    env.CreateParameters(context, settings.DatabaseKeyName, true);
                    context.SaveChanges();
                }
            }
            EnsureUriFormat(settings.ReplicationBusNamespaceFormat, settings.ReplicationUriBusKeyName, settings, env.ConfigSet,context);
            EnsureUriFormat(settings.DatabaseUriFormat, settings.DatabaseUriKeyName, settings, env.ConfigSet, context);
            EnsureUriFormat(settings.ServiceBusNamespaceFormat, settings.ServiceBusUriKeyName, settings, env.ConfigSet, context);
        }

        private static void EnsureUriFormat(string format, string propertyName, ConfigurationSettings settings, IConfigSet configSet, ConfigurationContext context)
        {
            var hostSettings = configSet.ServiceHosts.Single(s => String.Equals(s.Name, settings.DataCenterServiceHostName, StringComparison.OrdinalIgnoreCase));
            var param = hostSettings.Parameters.SingleOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (param.IsNull())
            {
                param=hostSettings.CreateParameter(context, propertyName, false, true);
                param.ItemValue = "{0}";
                if (param.IsEnvironmental)
                {
                    foreach (var environment in hostSettings.ConfigSet.Environments)
                    {
                        environment.CreateSubstitutionParameters(context, hostSettings.Name + "_" + param.Name);
                    }
                }
            }
            var env = configSet.Environments.Single(e => e.Name == settings.Environment);
            
            var subPar = env.SubstitutionParameters.SingleOrDefault(sp => sp.Name == hostSettings.Name + "_" + propertyName);
            if (subPar.IsNull())
            {
                subPar = env.CreateSubstitutionParameters(context, hostSettings.Name + "_" + propertyName);
            }
            subPar.ItemValue = format;
            context.SaveChanges();
        }

        private static void EnsureSubstitutionParameters(ConfigurationContext context, IEnvironment env)
        {
            foreach (var serviceHostSettingse in env.ConfigSet.ServiceHosts)
            {
                foreach (var serviceHostParameter in from p in serviceHostSettingse.Parameters where p.IsEnvironmental select p)
                {
                    if (
                        env.SubstitutionParameters.All(
                            s => s.Name != string.Format("{0}_{1}", serviceHostSettingse.Name, serviceHostParameter.Name)))
                    {
                        env.CreateSubstitutionParameters(
                            context,
                            string.Format("{0}_{1}", serviceHostSettingse.Name, serviceHostParameter.Name));
                        context.SaveChanges();
                    }
                }
            }
        }

        public static void AddToChildren(this IEnvironment env, ConfigurationContext context, IEnvironmentParameter newPar)
        {
            foreach (var childEnvironment in env.ChildEnvironments)
            {
                var e = childEnvironment;
                var param = newPar.CreateChild(context, ref e);
                context.SaveChanges();
                AddToChildren(e, context, param);
            }
        }
    }
}