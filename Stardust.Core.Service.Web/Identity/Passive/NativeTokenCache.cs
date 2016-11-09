using System;
using System.IO;
using System.Security.Cryptography;
using System.Web.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Stardust.Particles;

namespace Stardust.Core.Service.Web.Identity.Passive
{
    public class NativeTokenCache : TokenCache
    {
        public static TokenCache DefaultTokenCache
        {
            get
            {
                if (tokenCache == null)
                    tokenCache = new NativeTokenCache();
                return tokenCache;
            }
        }
        public string CacheFilePath;
        private static readonly object FileLock = new object();

        private static NativeTokenCache tokenCache;

        // Initializes the cache against a local file.
        // If the file is already present, it loads its content in the ADAL cache
        public NativeTokenCache(string filePath = @"\TokenCache.dat")
        {
            var pathBase = ConfigurationManagerHelper.GetValueOnKey("stardust.nativeTokenCachePath");
            if (pathBase.ContainsCharacters()) CacheFilePath = pathBase + filePath;
            else CacheFilePath = AppDomain.CurrentDomain.BaseDirectory + "App_Data" + filePath;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            lock (FileLock)
            {
                try
                {
                    this.Deserialize(File.Exists(CacheFilePath) ?
                                                 MachineKey.Unprotect(File.ReadAllBytes(CacheFilePath))
                                                 : null);
                }
                catch (CryptographicException ex)
                {
                    ex.Log();
                    if (!File.Exists(CacheFilePath)) throw;
                    File.Delete(CacheFilePath);
                    this.Deserialize(File.Exists(CacheFilePath) ?
                                                 MachineKey.Unprotect(File.ReadAllBytes(CacheFilePath))
                                                 : null);
                }
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            File.Delete(CacheFilePath);
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                this.Deserialize(File.Exists(CacheFilePath) ?
                                     MachineKey.Unprotect(File.ReadAllBytes(CacheFilePath))
                                     : null);
            }
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                lock (FileLock)
                {
                    Logging.DebugMessage("Protect {0}", args.DisplayableId);
                    // reflect changes in the persistent store
                    File.WriteAllBytes(CacheFilePath, MachineKey.Protect(this.Serialize()));
                    // once the write operation took place, restore the HasStateChanged bit to false
                    this.HasStateChanged = false;
                }
            }
        }
    }
}