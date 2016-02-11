//
// wcfrequeststate.cs
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

using System;
using Stardust.Core;
using Stardust.Core.Wcf;

namespace Stardust.Nucleus.ContextProviders
{
    internal class WcfRequestState : IRequestState
    {
        public object Get(Type key)
        {
            return ThreadSynchronizationContext.CurrentContext.GetItemFromContext(key.FullName);
        }

        public void Remove(Type key)
        {
            ThreadSynchronizationContext.CurrentContext.RemoveItemFromContext(key.FullName);
        }

        public void RemoveAll()
        {
            ThreadSynchronizationContext.CurrentContext.ClearContext();
        }

        public void Store(Type key, object something)
        {
            ThreadSynchronizationContext.CurrentContext.SetItemInContext(key.FullName, something);
            RegisterForDispose(something);
        }

        private static void RegisterForDispose(object something)
        {
            var item = something as IDisposable;
            if (item != null)
                ThreadSynchronizationContext.CurrentContext.RegisterForDispose(item);
        }
    }
}