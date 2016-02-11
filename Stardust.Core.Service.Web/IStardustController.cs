using System;
using Stardust.Interstellar;
using Stardust.Interstellar.Trace;

namespace Stardust.Core.Service.Web
{
    public interface IStardustController
    {
        IRuntime Runtime { get; }

        bool DoInitializationOnActionInvocation { get; }

        string GetMethodName(Uri requestUri, string action);

        string GetServiceName(Uri requestUri);


        void SetTracer(ITracer tracer);
    }
}