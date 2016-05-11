using System.Collections.Generic;
using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    public interface IServiceParameterResolver
    {
        IEnumerable<ParameterWrapper> ResolveParameters(MethodInfo methodInfo);
    }
}