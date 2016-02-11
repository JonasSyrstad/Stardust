using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stardust.Nucleus.Configuration;
using Stardust.Nucleus.Internals;
using Stardust.Particles;

namespace Stardust.Nucleus.TypeResolver
{
    internal class TypeResolverFromConfiguration : IConfigurationTypeResolver
    {
        public IEnumerable<IScopeContext> GetTypeBindingsFromConfig(Type type)
        {
            var items = GetConfigItems(type);
            var list = items.Select(GetContextFromItem).ToList();
            return list;
        }

        private IScopeContext GetContextFromItem(ImplementationDefinition item)
        {
            return item.IsNull()
                ? ScopeContext.NullInstance()
                : ResolveType(item);
        }

        private static IScopeContext ResolveType(ImplementationDefinition item)
        {
            if (item.AssemblyPath.ContainsCharacters() && File.Exists(item.AssemblyPath))
            {
                var type = Assembly.LoadFile(item.AssemblyPath).GetType(item.TypeFullName, true, true);
                new ScopeContext(type, item.ImplementationKey) { AllowOverride = false }.SetScope(item.Scope);
                
            }
            return new ScopeContext(item.type,item.ImplementationKey) { AllowOverride = false }.SetScope(item.Scope);
        }

        private static IEnumerable<ImplementationDefinition> GetConfigItems(Type module)
        {
            if (ConfigurationHelper.Configurations.Value.IsNull()) return new List<ImplementationDefinition>();
            var configModule = from m in ConfigurationHelper.Configurations.Value.ModuleCreators.OfType<MappingElement>()
                               where  m.BaseModuleName == module.FullName
                               from im in m.Implementations.OfType<ImplementationDefinition>()
                               select im;
            return configModule;
        }
    }

}