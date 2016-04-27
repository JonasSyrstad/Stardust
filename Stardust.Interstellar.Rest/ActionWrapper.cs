using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Stardust.Interstellar.Rest
{
    internal class ActionWrapper
    {
        public string Name { get; set; }

        public Type ReturnType { get; set; }

        public List<ParameterWrapper> Parameters { get; set; }

        public string RouteTemplate { get; set; }

        public List<HttpMethod> Actions { get; set; }
    }
}