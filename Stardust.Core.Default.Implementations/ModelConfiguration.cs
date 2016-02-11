using System.Collections.Generic;
using ProtoBuf.Meta;

namespace Stardust.Core.Default.Implementations
{
    public sealed class ModelConfiguration<T> : IModelConfigurationContext<T>
    {
        private readonly ModelConfigurationContext<T> ModelConfigurationContext;
        private readonly MetaType MetaType;

        internal ModelConfiguration(ModelConfigurationContext<T> modelConfigurationContext, MetaType metaType)
        {
            ModelConfigurationContext = modelConfigurationContext;
            MetaType = metaType;
        }

        public MetaType Metatype { get { return MetaType; } }

        public ModelConfiguration<T> AddTypeToModel()
        {
            return ModelConfigurationContext.AddTypeToModel();
        }

        public ModelConfiguration<T> AddTypeToModel<TType>()
        {
            return ModelConfigurationContext.AddTypeToModel<TType>();
        }

        public ModelConfiguration<T> AddListToModel<TList, TType>() where TList : IEnumerable<TType>
        {
            return ModelConfigurationContext.AddListToModel<TList, TType>();
        }

        public ModelConfiguration<T> AddListToModel<TList>() where TList : IEnumerable<T>
        {
            return ModelConfigurationContext.AddTypeToModel<TList>();
        }

        public void Compile()
        {
            ModelConfigurationContext.Compile();
        }
    }
}