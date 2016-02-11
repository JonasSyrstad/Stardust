using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Stardust.Particles;
using Stardust.Nucleus;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class EndpointParameter
    {
        public override string ToString()
        {
            return Id;
        }

        private Dictionary<string, string> EnvironmentalValueCache;
        public string Value
        {
            get
            {
                if (ItemValue == null)
                {
                    return Parent == null ? "" : Parent.Value;
                }
                return ItemValue;
            }
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

        public bool IsRoot { get { return Parent == null; } }

        private void BuildEnvironmentSubstitutionCache()
        {
            EnvironmentalValueCache = new Dictionary<string, string>();
            foreach (var environment in Endpoint.ServiceDescription.ConfigSet.Environments)
            {
                var substitute = "";
                if (IsPerService)
                    substitute = GetPerServiceSubstituteValue(environment);
                else
                    substitute = GetSubstituteValue(environment, substitute);
                EnvironmentalValueCache.Add(environment.Name, substitute);
            }
        }

        private string GetSubstituteValue(IEnvironment environment, string substitute)
        {
            if (Value == null) return substitute;
            var val = environment.SubstitutionParameters.FirstOrDefault(x => String.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase));
            try
            {
                substitute = string.Format(Value, val != null ? val.Value : null);
            }
            catch (System.Exception)
            {


            }
            return substitute;
        }

        private string GetPerServiceSubstituteValue(IEnvironment environment)
        {
            var subValueName = string.Format("{0}_{1}", Endpoint.ServiceDescription.Name, Name);
            var host = Endpoint.ServiceDescription.ServiceHost;
            if (host.IsInstance())
            {
                var lookupValue = Name;
                if (lookupValue == "Audience") lookupValue = "Address";
                var formatedValue = host.Parameters.SingleOrDefault(p => string.Equals(p.Name, lookupValue, StringComparison.OrdinalIgnoreCase));
                var envParam =
                    environment.SubstitutionParameters.SingleOrDefault(
                        p => string.Equals(p.Name, string.Format("{0}_{1}", host.Name, lookupValue), StringComparison.OrdinalIgnoreCase));
                if (envParam.IsInstance() && formatedValue.IsInstance())
                {
                    var hostValue = string.Format(formatedValue.Value, envParam.Value);
                    return string.Format(Value, hostValue);
                }
            }
            var subValue = environment.SubstitutionParameters.SingleOrDefault(x => x.Name == subValueName);
            var substitute = "";
            if (subValue != null)
            {
                try
                {
                    substitute = string.Format(Value, subValue.Value);
                }
                catch (System.Exception)
                {
                }
            }
            else
            {
                CreateSubstitutionParameterForEnvironment(environment, subValueName);
            }
            return substitute;
        }

        private static void CreateSubstitutionParameterForEnvironment(IEnvironment environment, string subValueName)
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

        public static void CreateSubstitutionParameterForEnvironment(IEnvironment environment, string subValueName, string itemValue)
        {
            ConfigurationContext context;
            var supParam = CreateSubstitutionParameterCore(environment, subValueName, out context);
            supParam.ItemValue = itemValue;
            context.SaveChanges();
        }
    }
}