using System;
using System.ServiceModel.Dispatcher;
using Stardust.Interstellar.Messaging;
using Stardust.Particles;
using Stardust.Interstellar;

namespace Stardust.Core.Wcf
{
    public class InstanceIdentityInspector : IParameterInspector
    {
        public object BeforeCall(string operationName, object[] inputs)
        {
            if (inputs.IsNull()) return null;
            foreach (var input in inputs)
            {
                var message = input as IRequestBase;
                if (message.IsInstance())
                {
                    message.MethodName = operationName;
                    var runtime = RuntimeFactory.CreateRuntime();
                    message.RuntimeInstance = RuntimeFactory.GetInstanceId().ToString();
                    message.ServerIdentity = Environment.MachineName;
                    SetMessageValuesIfNotSetInClient(message, runtime);
                }
            }
            return null;
        }

        private static void SetMessageValuesIfNotSetInClient(IRequestBase message, IRuntime runtime)
        {
            if (message.MessageId.IsNullOrWhiteSpace())
                message.MessageId = Guid.NewGuid().ToString();
            if (message.Environment.IsNullOrWhiteSpace())
                message.Environment = runtime.Environment;
            if (message.ServiceName.IsNullOrWhiteSpace())
                message.ServiceName = runtime.ServiceName;
            if (message.ConfigSet.IsNullOrWhiteSpace())
                message.ConfigSet = ConfigurationManagerHelper.GetValueOnKey("configSet");
            if (message.TimeStamp == null)
                message.TimeStamp = DateTime.UtcNow;
            if (runtime.RequestContext.IsInstance())
                message.ReferingMessageId = runtime.RequestContext.MessageId;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }
    }
}