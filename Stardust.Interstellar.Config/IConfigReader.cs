using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Interstellar.Rest;

namespace Stardust.Interstellar.Config
{
    [StardustConfigAuthentication]
    public interface IConfigReader
    {
        [HttpGet]
        [Route("api/ConfigReader/{id}")]
        
        ConfigurationSet Get([In(InclutionTypes.Header)] string id, [In(InclutionTypes.Header)]string env = null, [In(InclutionTypes.Header)]string updKey = null);
    }
}
