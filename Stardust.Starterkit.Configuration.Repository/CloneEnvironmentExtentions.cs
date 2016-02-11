using System;
using System.Linq;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public static class CloneEnvironmentExtentions
    {
        public static IEnvironment CreateChild(this IEnvironment environment, ConfigurationContext context, ref IConfigSet configSet)
        {
            var newEnvironment = context.Environments.Create();
            newEnvironment.ConfigSetNameId = configSet.Id;
            newEnvironment.ConfigSet = configSet;
            newEnvironment.ParentEnvironment = environment;
            newEnvironment.Name = environment.Name;
            configSet.Environments.Add(newEnvironment);
            context.SaveChanges();
            foreach (var substitutionParameter in environment.SubstitutionParameters)
            {
                var child = CreateChild(substitutionParameter, context, ref newEnvironment);
                if (newEnvironment.SubstitutionParameters.All(sp => !string.Equals(sp.Name, substitutionParameter.Name, StringComparison.OrdinalIgnoreCase)))
                    newEnvironment.SubstitutionParameters.Add(child);
            }
            foreach (var service in configSet.Services)
            {
                var addressOverride = string.Format("{0}_Address", service.Name);
                var overrideProp = newEnvironment.SubstitutionParameters.SingleOrDefault(x => string.Equals(x.Name, addressOverride, StringComparison.OrdinalIgnoreCase));
                if (overrideProp.IsNull())
                    newEnvironment.CreateSubstitutionParameters(context, addressOverride);
            }
            foreach (var environmentParameter in environment.EnvironmentParameters)
            {
                newEnvironment.EnvironmentParameters.Add(CreateChild(environmentParameter, context, ref newEnvironment));
            }
            return newEnvironment;
        }

        public static IEnvironmentParameter CreateChild(this IEnvironmentParameter substitutionParameter, ConfigurationContext context, ref IEnvironment newEnvironment)
        {
            var envParameter = context.EnvironmentParameters.Create();
            envParameter.EnvironmentNameId = newEnvironment.Id;
            envParameter.Environment = newEnvironment;
            envParameter.Name = substitutionParameter.Name;
            envParameter.Parent = substitutionParameter;
            envParameter.IsSecureString = substitutionParameter.IsSecureString;
            context.SaveChanges();
            return envParameter;
        }

        private static ISubstitutionParameter CreateChild(this ISubstitutionParameter substitutionParameter, ConfigurationContext context, ref IEnvironment newEnvironment)
        {
            var subParameter = context.SubstitutionParameters.Create();
            subParameter.EnvironmentNameId = newEnvironment.Id;
            subParameter.Environment = newEnvironment;
            subParameter.Name = substitutionParameter.Name;
            subParameter.Parent = substitutionParameter;
            context.SaveChanges();
            return subParameter;
        }
    }
}