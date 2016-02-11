using System.Collections.Generic;
using System.Linq;
using ProtoBuf.Meta;
using Stardust.Particles;

namespace Stardust.Core.Default.Implementations
{
    internal sealed class ModelConfigurationContext<T> : IModelConfigurationContext<T>
    {
        private readonly RuntimeTypeModel typeModel;

        internal ModelConfigurationContext(RuntimeTypeModel doCreateModel)
        {
            typeModel = doCreateModel;
        }

        public ModelConfiguration<T> AddTypeToModel()
        {
            return AddTypeToModel<T>();
        }

        public ModelConfiguration<T> AddTypeToModel<TType>()
        {
            var attrib = GetIgnoreAttributeOrDefailt();
            var properties = typeof(TType).GetProperties().Where(p => !attrib.IsIgnored(p.Name)).Select(p => p.Name).OrderBy(name => name);
            return new ModelConfiguration<T>(this, typeModel.Add(typeof(TType), true).Add(properties.ToArray()));
        }

        private static IIgnoreAttribute GetIgnoreAttributeOrDefailt()
        {
            var attrib = typeof (T).GetAttribute<IgnoreMembersAttribute>() as IIgnoreAttribute;
            if (attrib.IsNull())
                attrib = new DefaultIgnore();
            return attrib;
        }

        public ModelConfiguration<T> AddListToModel<TList, TType>() where TList : IEnumerable<TType>
        {
            return AddTypeToModel<TList>();
        }

        public ModelConfiguration<T> AddListToModel<TList>() where TList : IEnumerable<T>
        {
            return AddTypeToModel<TList>();
        }

        public void Compile()
        {
            typeModel.CompileAndCache<T>();
        }
    }
}