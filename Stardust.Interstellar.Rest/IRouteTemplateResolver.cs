using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    internal interface IRouteTemplateResolver
    {
        string GetTemplate(MethodInfo methodInfo);
    }
}