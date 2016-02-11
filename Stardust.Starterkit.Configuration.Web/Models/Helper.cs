using System.Collections.Generic;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Web.Models
{
    public static class Helper
    {
        public static string RemoveSortHelper(this string value)
        {
            return value.Replace("1 ", "").Replace("2 ", "");
        }

        public static List<BreadCrumbItem> GetTrail(this IEnvironment self)
        {
            return new List<BreadCrumbItem>
                       {
                           ConfigSet(self.ConfigSet)
                       };
        }

        private static BreadCrumbItem ConfigSet(IConfigSet self)
        {
            return new BreadCrumbItem
                       {
                           Url = string.Format("/ConfigSet/Details?name={0}&system={1}", self.Name, self.System),
                           Name = self.Id
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IServiceDescription self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.ConfigSet)
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IEndpoint self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.ServiceDescription.ConfigSet),
                            ServiceDescription(self.ServiceDescription)
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IServiceHostSettings self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.ConfigSet)
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IServiceHostParameter self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.ServiceHost.ConfigSet),
                            ServiceHost(self.ServiceHost)
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this ISubstitutionParameter self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.Environment.ConfigSet),
                            Environment(self.Environment)
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IConfigSet self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self),
                       };
        }

        public static List<BreadCrumbItem> GetTrail(this IEnvironmentParameter self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.Environment.ConfigSet),
                            Environment(self.Environment)
                       };
        }

        private static BreadCrumbItem Environment(IEnvironment self)
        {
            return new BreadCrumbItem
            {
                Url = string.Format("/Environment/Details/edit?item={0}", self.Id),
                Name = self.Name
            };
        }

        private static BreadCrumbItem ServiceHost(IServiceHostSettings self)
        {
            return new BreadCrumbItem
            {
                Url = string.Format("/ServiceHosts/Details/{0}", self.Id),
                Name = self.Name
            };
        }

        public static List<BreadCrumbItem> GetTrail(this IEndpointParameter self)
        {
            return new List<BreadCrumbItem>
                       {
                            ConfigSet(self.Endpoint.ServiceDescription.ConfigSet),
                            ServiceDescription(self.Endpoint.ServiceDescription),
                            Endpoint(self.Endpoint)
                       };
        }

        private static BreadCrumbItem Endpoint(IEndpoint self)
        {
            return new BreadCrumbItem
            {
                Url = string.Format("/Endpoint/Details/edit?item={0}", self.Id),
                Name = self.Name
            };
        }

        private static BreadCrumbItem ServiceDescription(IServiceDescription self)
        {
            return new BreadCrumbItem
            {
                Url = string.Format("/Service/Details/edit?item={0}", self.Id),
                Name = self.Name
            };
        }
    }

    public class BreadCrumbItem
    {
        public string Url { get; set; }

        public string Name { get; set; }
    }
}