using System.Collections.Generic;

namespace Stardust.Core.Default.Implementations
{
    public interface IModelConfigurationContext<T>
    {
        ModelConfiguration<T> AddTypeToModel();
        ModelConfiguration<T> AddTypeToModel<TType>();
        ModelConfiguration<T> AddListToModel<TList, TType>() where TList : IEnumerable<TType>;
        ModelConfiguration<T> AddListToModel<TList>() where TList : IEnumerable<T>;
        void Compile();
    }
}