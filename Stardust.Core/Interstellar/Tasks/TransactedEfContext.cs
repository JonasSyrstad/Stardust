//
// transactedefcontext.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Transactions;
using Stardust.Nucleus;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Interstellar.Tasks
{
    class TransactedEfContext<T> : ITransactedEfContext<T> where T : ObjectContext
    {
        private T Context;
        protected IRuntime Runtime;

        public T GetConnection(CommittableTransaction transaction = null)
        {
            if (Context.IsNull())
            {
                var type = typeof(T);
                if (transaction.IsNull())
                    Context = (T) ObjectFactory.Createinstance(type, new[] { Runtime.Context.GetDefaultConnectionString() });
                else
                {
                    var dataConnection = new EntityConnection(Runtime.Context.GetDefaultConnectionString());
                    dataConnection.Open();
                    dataConnection.EnlistTransaction(transaction);
                    Context = (T)ObjectFactory.Createinstance(type, new[] { dataConnection });
                }
            }
            return Context;
        }

        public IRuntimeTask SetExternalState(ref IRuntime runtime)
        {
            Runtime = runtime;
            return this;
        }

        public bool SupportsAsync
        {
            get { return false; }
        }

        public void SetCallStack(CallStackItem callStack)
        {
            CallStack = callStack.CallStack;
        }

        public IRuntimeTask SetInvokerStateStorage(IStateStorageTask stateItem)
        {
            return this;
        }

        public List<CallStackItem> CallStack { get; set; }

        public void SetCallStack(ConcurrentStack<CallStackItem> callStack)
        {
            throw new System.NotImplementedException();
        }

        public Ttask CreateTask<Ttask>() where Ttask : IRuntimeTask
        {
            throw new System.NotImplementedException();
        }

        public Ttask CreateTask<Ttask>(Scope scope) where Ttask : IRuntimeTask
        {
            throw new System.NotImplementedException();
        }

        public Ttask CreateTask<Ttask>(Scope scope, System.Type innerType) where Ttask : IRuntimeTask
        {
            throw new System.NotImplementedException();
        }
    }
}