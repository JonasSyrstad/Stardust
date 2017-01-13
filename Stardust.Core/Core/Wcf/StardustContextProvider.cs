using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Stardust.Particles;

namespace Stardust.Core.Wcf
{
    internal class StardustContextProvider:IDisposable
    {
        public StardustContextProvider()
        {
            Container = new ConcurrentDictionary<string, object>();
            DisposeList = new List<IDisposable>();
            Created=DateTime.UtcNow;
        }

        public List<IDisposable> DisposeList { get; private set; }

        public ConcurrentDictionary<string, object> Container { get; private set; }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            Container.Clear();
            DisposeList?.ForEach(i=>i.TryDispose());
            DisposeList?.Clear();
            if(disposing) GC.SuppressFinalize(this);
            if(DoLogging)Logging.DebugMessage("context.Dispose({0})",disposing);
        }
        private static bool DoLogging
        {
            get { return ConfigurationManagerHelper.GetValueOnKey("stardust.Debug") == "true"; }
        }

        public DateTime Created { get; private set; }
    }
}