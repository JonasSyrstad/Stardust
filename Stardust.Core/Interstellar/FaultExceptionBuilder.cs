using System.Net.Security;
using System.ServiceModel.Description;
using Stardust.Interstellar.Messaging;

namespace Stardust.Interstellar
{
    internal static class FaultExceptionBuilder
    {
        internal static void AddFaultContract(this OperationDescription operation)
        {
            operation.Faults.Add(CreateFaultDescription(operation));
        }

        internal static FaultDescription CreateFaultDescription(OperationDescription operation)
        {
            return new FaultDescription(operation.Name)
                       {
                           Name = "ErrorMessage",
                           Namespace = "http://stardustframework.com/messaging/fault",
                           DetailType = typeof(ErrorMessage),
                           ProtectionLevel = ProtectionLevel.None
                       };
        }
    }
}