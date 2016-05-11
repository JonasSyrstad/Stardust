using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    internal interface IWebMethodConverter
    {
        List<HttpMethod> GetHttpMethods(MethodInfo method);
    }
}