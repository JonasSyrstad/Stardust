using System.Web.Http;

namespace AuthenticationTestSite.Controllers.Api
{
    public class TestServiceController : ApiController,ITestService
    {
    }

    public interface ITestService
    {

    }
}
