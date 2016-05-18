using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    public interface IWebMethodConverter
    {
        List<HttpMethod> GetHttpMethods(MethodInfo method);
    }
}