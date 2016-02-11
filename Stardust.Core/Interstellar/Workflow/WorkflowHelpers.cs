//
// WorkflowHelpers.cs
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

using System.Data.Objects;
using System.Data.SqlClient;
using System.Transactions;
using Stardust.Interstellar.Tasks;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;

namespace Stardust.Interstellar.Workflow
{
    public static class WorkflowHelpers
    {
        public static void BeginTransaction(this IRuntime self)
        {
            self.GetServiceTransaction().CreateTransaction();
        }

        public static SqlConnection GetSqlConnection(this IRuntime self, Scope scope = Scope.Context)
        {
            return self.CreateRuntimeTask<ITransactedSqlConnection>(scope)
                .GetConnection(self.GetTransaction());
        }

        private static CommittableTransaction GetTransaction(this IRuntime self)
        {
            return self.GetServiceTransaction().GetTransaction();
        }

        public static T GetEntityContext<T>(this IRuntime self, Scope scope = Scope.Context) where T : ObjectContext
        {
            return self.CreateRuntimeTask<ITransactedEfContext<T>>(scope)
                .GetConnection(self.GetTransaction());
        }

        public static void Commit(this IRuntime self)
        {
            self.GetServiceTransaction().Commit();
        }

        public static void Rollback(this IRuntime self)
        {
            self.GetServiceTransaction().Rollback();
        }
    }
}
