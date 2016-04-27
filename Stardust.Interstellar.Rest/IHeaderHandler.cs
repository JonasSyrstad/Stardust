using System.Net;

namespace Stardust.Interstellar.Rest
{
    public interface IHeaderHandler
    {
        void SetHeader(HttpWebRequest req);
    }
}