using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Interstellar;
using Stardust.Interstellar.Tasks;

namespace Stardust.Core.ServicesTest.Mocks
{
    [ServiceName("ContactsService")]
    public interface ITestTask : IRuntimeTask
    {
        object CreateNewObject();
    }

    public class TestTask : ITestTask
    {
        private Action<Message> CallbackHandler;

        public IRuntimeTask SetExternalState(ref IRuntime runtime)
        {
            return this;
        }

        public bool SupportsAsync
        {
            get { return true; }
        }

        public virtual object CreateNewObject()
        {
            return new object(); 
        }

        public virtual Message ExecuteWrapper(Message message)
        {
            Thread.Sleep(19);
            return message;
        }

        public List<CallStackItem> CallStack { get; private set; }

        public IRuntimeTask SetInvokerStateStorage(IStateStorageTask stateItem)
        {
            return this;
        }
    }

   
}
