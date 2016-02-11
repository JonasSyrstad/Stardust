using System;
using Stardust.Nucleus.ContextProviders;
using Stardust.Particles;

namespace Stardust.Nucleus.TypeResolver
{
    internal class KernelContext : IKernelContext, IKernelContextCommands
    {
        private IControlledProvider Provider;
        public KernelContext(IControlledProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            if (Provider.IsNull()) return;
            ClearExtendedScope();
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~KernelContext()
        {
            Dispose(false);
        }

        void IKernelContextCommands.End()
        {
            ClearExtendedScope();
            Dispose();
        }

        private void ClearExtendedScope()
        {
            Provider.EndScope();
            Provider = null;
        }
    }
}