using System;
using System.Collections.Generic;
using Autofac;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.AutoFac
{
    internal class AutoFacKernelWrapper : IConfigurationKernel
    {
        private ContainerBuilder Builder;

        public IScopeContext Resolve(Type type, string named, bool skipAlternateResolving = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IScopeContext> ResolveAll(Type type)
        {
            throw new NotImplementedException();
        }

        public void Bind(Type concreteType, IScopeContext existingBinding, string identifier)
        {
            throw new NotImplementedException();
        }

        public void Unbind(Type type, string identifier)
        {
            throw new NotImplementedException();
        }

        public void UnbindAll(Type type)
        {
            throw new NotImplementedException();
        }

        public void Unbind(Type type, IScopeContext scopeContext, string identifier)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, string>> ResolveList(Type type)
        {
            throw new NotImplementedException();
        }

        public void UnbindAll()
        {
            throw new NotImplementedException();
        }

        public ContainerBuilder GetAutoFacConfigItem()
        {
            if (Builder.IsNull())
                Builder = new ContainerBuilder();
            return Builder;
        }
    }
}