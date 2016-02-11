using System.Web.Mvc;

namespace Stardust.Core.Services.ConfigurationAdministration
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}