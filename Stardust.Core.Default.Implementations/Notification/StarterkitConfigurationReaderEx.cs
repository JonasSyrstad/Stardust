using System;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Core.Default.Implementations.Notification
{
    class StarterkitConfigurationReaderEx : StarterkitConfigurationReader
    {
        private static Action<ConfigurationSet> handler;

        /// <summary>
        /// This will only be used if the Reader implementation supports change notification. not implemented in this, but if you change the cache by overriding this remember to hook up this.
        /// </summary>
        /// <param name="onCacheChanged"/>
        public override void SetCacheInvalidatedHandler(Action<ConfigurationSet> onCacheChanged)
        {
            handler = onCacheChanged;

        }

        internal static void Notify(ConfigurationSet configSet)
        {
            if (handler != null) handler(configSet);
        }
    }
}