using System;
using System.Net;
using System.Runtime.Serialization;

namespace Stardust.Interstellar.Rest
{
    [Serializable]
    internal class RestWrapperException<T> : RestWrapperException
    {
        public RestWrapperException()
        {
        }

        public RestWrapperException(string message) : base(message)
        {
        }

        public RestWrapperException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RestWrapperException(string message, HttpStatusCode httpStatus, object response, Exception error) : base(message, httpStatus, response, error)
        {
        }

        protected RestWrapperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new T Response => (T)base.Response;
    }
}