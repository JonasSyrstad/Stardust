using System.Linq;
using Newtonsoft.Json;
using Stardust.Accretion;

namespace Stardust.Stardust_Tooling
{
    public static class Helpers
    {
        public static Rootobject GetConfig(this ICodeWriterContext context, string content)
        {
            return JsonConvert.DeserializeObject<Rootobject>(content);
        }

        public static Environment[] GetEnvironments(this ICodeWriterContext context, string content)
        {
            return JsonConvert.DeserializeObject<Rootobject>(content).Environments;
        }

        public static string[] GetParameters(this Environment[] environments)
        {

            return (from e in environments
                    from p in e.Parameters
                    where string.IsNullOrEmpty(GetBinary(p))
                    select p.Name.Trim()).ToArray().Distinct().ToArray();
        }
        

        private static string GetBinary(Parameter1 p)
        {
            return p.BinaryValue==null ? null : p.BinaryValue.Trim();
        }

        public static string[] GetSecureParameters(this Environment[] environments)
        {
            return (from e in environments from p in e.Parameters where !string.IsNullOrEmpty(p.BinaryValue) select p.Name).Distinct().ToArray();
        }
    }
}