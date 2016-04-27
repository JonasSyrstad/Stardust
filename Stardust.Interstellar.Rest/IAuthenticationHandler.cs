using System.Net;

namespace Stardust.Interstellar.Rest
{
    public interface IAuthenticationHandler
    {
        void Apply(HttpWebRequest req);
    }
}