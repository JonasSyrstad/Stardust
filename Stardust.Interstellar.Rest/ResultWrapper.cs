using System;
using System.Net;

namespace Stardust.Interstellar.Rest
{
    public class ResultWrapper
    {
        public bool IsVoid { get; set; }

        public object Value { get; set; }

        public Type Type { get; set; }

        public HttpStatusCode Status { get; set; }

        public string StatusMessage { get; set; }

        public Exception Error { get; set; }
    }
}