using System.ServiceModel;
using System.ServiceModel.Web;
using Stardust.Interstellar;

namespace AuthenticationTestSite
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITestRest" in both code and config file together.
    [ServiceContract]
    [ServiceName("SecureRestTestService")]
    public interface ITestRest
    {
        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json,ResponseFormat = WebMessageFormat.Json,BodyStyle = WebMessageBodyStyle.Bare,UriTemplate = "json/{id}")]
        ComplexType DoWork(string id);
    }
}
