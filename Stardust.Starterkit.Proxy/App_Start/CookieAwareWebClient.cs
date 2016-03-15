using System;
using System.Net;

namespace Stardust.Starterkit.Configuration.Web
{
    internal class CookieAwareWebClient : WebClient
    {
        internal CookieContainer Container;

        private bool initialized;

        protected override WebRequest GetWebRequest(Uri address)
        {

            if (!initialized)
                Proxy = WebProxy.GetDefaultProxy();
            initialized = true;
            var request = base.GetWebRequest(address);
            var webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                if (Container != null)
                {
                    webRequest.CookieContainer = Container;
                }
            }
            return request;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Net.WebResponse"/> for the specified <see cref="T:System.Net.WebRequest"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Net.WebResponse"/> containing the response for the specified <see cref="T:System.Net.WebRequest"/>.
        /// </returns>
        /// <param name="request">A <see cref="T:System.Net.WebRequest"/> that is used to obtain the response. </param>
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Net.WebResponse"/> for the specified <see cref="T:System.Net.WebRequest"/> using the specified <see cref="T:System.IAsyncResult"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Net.WebResponse"/> containing the response for the specified <see cref="T:System.Net.WebRequest"/>.
        /// </returns>
        /// <param name="request">A <see cref="T:System.Net.WebRequest"/> that is used to obtain the response.</param><param name="result">An <see cref="T:System.IAsyncResult"/> object obtained from a previous call to <see cref="M:System.Net.WebRequest.BeginGetResponse(System.AsyncCallback,System.Object)"/> .</param>
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            return base.GetWebResponse(request, result);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Net.WebClient.DownloadStringCompleted"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Net.DownloadStringCompletedEventArgs"/> object containing event data.</param>
        protected override void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
        {
            base.OnDownloadStringCompleted(e);
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                var cookies = response.Cookies;
                Cookies = response.Cookies;
                if (Container != null)
                {
                    Container.Add(cookies);
                }
            }
        }

        public CookieCollection Cookies { get; private set; }
    }
}