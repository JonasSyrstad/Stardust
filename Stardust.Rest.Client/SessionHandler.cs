using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Stardust.Rest.Client
{
    public class SessionHandler
    {
        private string proxy;

        private readonly string applicationId;

        private IWebProxy MPx;
        private string MHostname;

        private string MSessionToken;

        private string lastExternalError;

        private static bool ignoreSerializationExceptions;

        public string MSessionid { get; private set; }

        public SessionHandler(string hostname, string proxy, string applicationId)
        {
            MHostname = hostname;
            this.proxy = proxy;
            this.applicationId = applicationId;
            MPx = proxy != null ? new WebProxy { Address = new Uri(proxy) } : WebRequest.DefaultWebProxy;
        }

        internal WebMethod CreateMethod(Method method, string api, bool isfirstparam)
        {
            return new WebMethod(this, method, api, isfirstparam);
        }


        public async Task<object> PostDataAsync(string api, object data, Type resultType)
        {
            using (var req = CreateMethod(Method.POST, api, false))
            {
                try
                {
                    var stream = await req.GetRequestStreamAsync();
                    SerializeGenericRequestMessage(data, stream);
                    await req.ExecuteAsync();
                    var returnValue = DeserializeGenericMessage(resultType, req.GetResponseStream());
                    return returnValue;
                }
                catch (WebException ex)
                {
                    return HandleWebException(ex);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public object PostData(string api,  object data,  Type resultType)
        {
            using (var req = CreateMethod(Method.POST, api, false))
            {
                try
                {
                    var stream = req.GetRequestStream();
                    SerializeGenericRequestMessage(data, stream);
                    req.Execute();
                    var returnValue = DeserializeGenericMessage(resultType, req.GetResponseStream());
                    return returnValue;
                }
                catch (WebException ex)
                {
                    return HandleWebException(ex);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        private object HandleWebException(WebException ex)
        {

            using (var response = ex.Response)
            {
                var httpResponse = response as HttpWebResponse;
                GrabErrorBody(response);
                if (httpResponse == null)
                {
                    throw ex;
                }
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException("Unauthorized access", ex);
                    case HttpStatusCode.ServiceUnavailable:
                        throw new ServiceException("Service unavailable", ex);
                    case HttpStatusCode.BadRequest:
                        throw new ServiceException("Invalid request", ex);
                    case HttpStatusCode.BadGateway:
                        throw new ServiceException("Bad bad gateway", ex);
                    case HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException("Unauthorized access", ex);
                    case HttpStatusCode.NoContent:
                        return new object();
                    default:
                        throw new ServiceException(string.Format("Unable to communicate with oracle ({0})", httpResponse.StatusCode), ex);
                }

            }
        }

        private void GrabErrorBody(WebResponse response)
        {
            try
            {
                if (response.ContentLength <= 0) return;
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    lastExternalError = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }
        }

        private static object DeserializeGenericMessage(Type resultType, Stream stream)
        {
            var serializer = CreateSerializer();
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var returnValue = serializer.Deserialize(new JsonTextReader(reader), resultType);
                    return returnValue;
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(ex.Message, ex);
            }
        }

        static void OnSerializerError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            if(ignoreSerializationExceptions)
                e.ErrorContext.Handled = true;
        }

        private void SerializeGenericRequestMessage(object data, Stream stream)
        {
            var serializer = CreateSerializer();
            using (var writer = new StreamWriter(stream))
            {
                serializer.Serialize(new JsonTextWriter(writer), data);

            }
        }
        public static MemoryStream SerializeToStream<T>(T instance, JsonSerializer serializer)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;
                    serializer.Serialize(new JsonTextWriter(writer), instance);
                    stream.Position = 0;
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)(stream.Length - 1));
                    return new MemoryStream(buffer);
                }

            }
        }

        public static string Serialize<T>(T instance, JsonSerializer serializer)
        {
            using (var stream = SerializeToStream(instance, serializer))
            {
                return ReadStringFromStream(stream);
            }
        }
        private static string ReadStringFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            var serializer = new JsonSerializer
                                 {
                                     DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                     DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                                     NullValueHandling = NullValueHandling.Include,
                                 };
            serializer.Error += OnSerializerError;
            return serializer;
        }

        public class WebMethod : IDisposable
        {
            private readonly HttpWebRequest req;
            private WebResponse resp;

            public WebMethod(SessionHandler session, Method method, string uri, bool isfirstparam)
            {
                req = WebRequest.Create(new Uri(string.Format("{0}/webservices/rest/{1}", session.MHostname, uri))) as HttpWebRequest;
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Accept = "application/json";
                req.ContentType = "application/json";
                req.Headers.Add("Accept-Language", "en-us");
                req.UserAgent = "TERS/1.0";
                req.Headers["Cookie"] = session.MSessionid.Replace(",", ";");
                req.Method = method.ToString();
                req.ContinueTimeout = 2;
                if (session.MPx != null)
                    req.Proxy = session.MPx;
            }

            /// <summary>
            /// Synchronously executes Http requests
            /// </summary>
            public void Execute()
            {
                resp = req.GetResponse() as HttpWebResponse;
            }

            public async Task ExecuteAsync()
            {
                resp = await req.GetResponseAsync();
            }


            /// <summary>
            /// Sets request header
            /// </summary>
            public void SetHeader(string name, string value)
            {
                req.Headers[name] = value;
            }

            public void SetContentLength(int length)
            {
                req.ContentLength = length;
            }

            /// <summary>
            /// Returns response stream
            /// </summary>
            /// <returns></returns>
            public Stream GetResponseStream()
            {
                return resp.GetResponseStream();
            }


            /// <summary>
            /// Returns request stream
            /// </summary>
            /// <returns></returns>
            public Task<Stream> GetRequestStreamAsync()
            {
                return req.GetRequestStreamAsync();
            }

            /// <summary>
            /// Returns request stream
            /// </summary>
            /// <returns></returns>
            public Stream GetRequestStream()
            {
                return req.GetRequestStream();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                if (resp!=null)
                    resp.Dispose();
            }
        }
    }
}