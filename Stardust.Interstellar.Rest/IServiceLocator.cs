using System.Collections.Generic;

namespace Stardust.Interstellar.Rest
{
    public interface IServiceLocator
    {
        T GetService<T>();

        IEnumerable<T> GetServices<T>();
    }
}