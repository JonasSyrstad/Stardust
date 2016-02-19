using System;
using System.Net;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Core.Default.Implementations.Notification
{
    internal class StarterkitConfigurationReaderEx : StarterkitConfigurationReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StarterkitConfigurationReaderEx()
        {
            Logging.DebugMessage("starting");
        }

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

        protected override WebClient GetClient()
        {
            return new CookieAwareWebClient();
        }
    }
}