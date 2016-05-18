using System.Reflection;

namespace Stardust.Interstellar.Rest
{
    public interface IRouteTemplateResolver
    {
        string GetTemplate(MethodInfo methodInfo);
    }
}