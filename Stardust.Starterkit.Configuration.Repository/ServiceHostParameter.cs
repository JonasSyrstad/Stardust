using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Nucleus;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class ServiceHostParameter
    {
        public string Value
        {
            get
            {
                if (ItemValue == null)
                    return Parent == null ? "" : Parent.Value;
                return ItemValue;
            }
        }

        public bool IsRoot { get { return Parent == null; } }

        public bool IsInherited
        {
            get { return string.IsNullOrWhiteSpace(ItemValue) && !IsRoot; }
        }

        public ConfigParameter GetRawConfigData(string environment)
        {
            var binary = new byte[0];
            try
            {
                if (Value.IsBase64String())
                    binary = Convert.FromBase64String(Value);
            }
            catch
            {
            }
            return new ConfigParameter
            {
                Name = Name,
                
                Value = !IsEnvironmental?Value:GetEnvironmentValue(this,environment),
                BinaryValue = binary,
                ChildParameters = new List<ConfigParameter>(),
                Description = Description
            };
        }

        private static string GetEnvironmentValue(IServiceHostParameter serviceHostParameter, string environment)
        {
            var envValue=serviceHostParameter.ServiceHost.ConfigSet.Environments.Single(e => string.Equals(e.Name, environment, StringComparison.OrdinalIgnoreCase))
                .SubstitutionParameters.SingleOrDefault(p => string.Equals(p.Name, string.Format("{0}_{1}", serviceHostParameter.ServiceHost.Name, serviceHostParameter.Name), StringComparison.OrdinalIgnoreCase));
            return envValue.IsInstance() ? string.Format(serviceHostParameter.Value, envValue.Value) : serviceHostParameter.Value;
        }

        public void SetValue(string value)
        {
            if (!IsSecureString)
            {
                ItemValue = value;
                return;
            }
            var encrypted = value.Encrypt(KeyHelper.SharedSecret); 
            ItemValue = encrypted;
            BinaryValue = encrypted.GetByteArray();
            if (ItemValue.Decrypt(KeyHelper.SharedSecret) != value) throw new StardustCoreException("Encryption validation failed!");
        }


        public override string ToString()
        {
            return Id;
        }

        public Dictionary<string, string> EnvironmentalValue
        {
            get
            {
                if (EnvironmentalValueCache == null)
                {
                    BuildEnvironmentSubstitutionCache();
                }
                return EnvironmentalValueCache;
            }
        }
        private Dictionary<string, string> EnvironmentalValueCache;
        private void BuildEnvironmentSubstitutionCache()
        {
            EnvironmentalValueCache = new Dictionary<string, string>();
            foreach (var environment in ServiceHost.ConfigSet.Environments)
            {
                var substitute = "";
                if (IsEnvironmental)
                    substitute = GetPerServiceSubstituteValue(environment);
                EnvironmentalValueCache.Add(environment.Name, substitute);
            }
        }

        private string GetPerServiceSubstituteValue(IEnvironment environment)
        {
            var subValueName = string.Format("{0}_{1}", ServiceHost.Name, Name);
            var subValue = environment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, subValueName, StringComparison.OrdinalIgnoreCase));
            var substitute = "";
            if (subValue != null)
            {
                substitute = string.Format(Value, subValue.Value);
            }
            else
            {
                CreateSubstitutionParameterForEnvironment(environment, subValueName);
            }
            return substitute;
        }

        public static void CreateSubstitutionParameterForEnvironment(IEnvironment environment, string subValueName)
        {
            ConfigurationContext context;
            CreateSubstitutionParameterCore(environment, subValueName, out context);
            context.SaveChanges();
        }

        private static ISubstitutionParameter CreateSubstitutionParameterCore(IEnvironment environment, string subValueName,
            out ConfigurationContext context)
        {
            context = (ConfigurationContext)ContainerFactory.Current.Resolve(typeof(ConfigurationContext), Scope.Context);
            if (context.IsNull()) throw new CommunicationException("Unable to get database");
            var subValue = context.SubstitutionParameters.Create();
            subValue.Name = subValueName;
            subValue.Environment = environment;
            subValue.EnvironmentNameId = environment.Id;
            return subValue;
        }
    }
}