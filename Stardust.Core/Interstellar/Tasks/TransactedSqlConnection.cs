//
// TransactedSqlConnection.cs
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

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using Stardust.Particles;

namespace Stardust.Interstellar.Tasks
{
    public class TransactedSqlConnection : ITransactedSqlConnection
    {
        private IRuntime Runtime;
        private SqlConnection Connection;
        private SqlTransaction Transaction;

        public SqlConnection GetConnection(CommittableTransaction transaction = null)
        {
            if (Connection.IsNull())
            {
                Connection = new SqlConnection(Runtime.Context.GetDefaultConnectionString());
                if (transaction.IsInstance())
                {
                    Connection.Open();
                    Connection.EnlistTransaction(transaction);
                }
            }
            return Connection;
        }

        void ITransactedSqlConnection.BeginTransaction()
        {
            if (Transaction.IsNull())
                Transaction = GetConnection().BeginTransaction();

        }

        public void Commit()
        {
            if (Transaction.IsInstance())
                Transaction.Commit();
            Transaction = null;
        }

        public void Rollback()
        {
            if (Transaction.IsInstance())
                Transaction.Rollback();
            Transaction = null;
        }

        public IRuntimeTask Initialize(string requestId, string configSetPath)
        {
            throw new System.NotImplementedException();
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

        public List<CallStackItem> CallStack { get; private set; }


        public IRuntimeTask SetInvokerStateStorage(IStateStorageTask stateItem)
        {
            return this;
        }
    }
}