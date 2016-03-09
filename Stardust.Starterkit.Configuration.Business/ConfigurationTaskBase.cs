using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Stardust.Core.Security;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public abstract class ConfigurationTaskBase
    {
        private readonly IRepositoryFactory factory;

        private ConfigurationContext repository;

        protected ConfigurationContext Repository
        {
            get
            {
                if (repository == null) repository = factory.GetRepository();
                return repository;
            }
        }

        protected static EncryptionKeyContainer KeySalt
        {
            get
            {
                return new EncryptionKeyContainer("makeItHarderTowrite");
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected ConfigurationTaskBase(IRepositoryFactory factory)
        {
            this.factory = factory;
        }

        protected IConfigSet GetConfigsetInternal(string id)
        {
            var configSets = from cs in Repository.ConfigSets where cs.Id == id select cs;

            var configSet = configSets.SingleOrDefault();
            if(configSet==null) throw new ArgumentOutOfRangeException(id,string.Format("No config set with id {0} is found", id));
            return configSet;
        }

        protected void AddToCache(string id, string environment, ConfigurationSet set)
        {
            ConfigSetCache.AddOrUpdate(GetCacheKey(id, environment), set);
        }



        private static ConcurrentDictionary<string, ConfigurationSet> ConfigSetCache = new ConcurrentDictionary<string, ConfigurationSet>(new Dictionary<string, ConfigurationSet>());

        protected bool TryGetSetFromCache(string id, string environment, out ConfigurationSet set)
        {
            ConfigurationSet item;
            if (!ConfigSetCache.TryGetValue(GetCacheKey(id, environment), out item))
            {
                set = null;
                return false;
            }
            if (item.LastUpdated <= GetConfigsetInternal(id).LastUpdate)
            {
                set = null;
                return false;
            }
            set = item;
            return true;
        }

        protected static string GetCacheKey(string id, string environment)
        {
            return string.Format("{0}[{1}]", id, environment);
        }

        public void UpdateSubstitutionParameter(ISubstitutionParameter parameter)
        {
            parameter.Environment.ConfigSet.LastUpdate = DateTime.UtcNow;

            Repository.SaveChanges();
        }
    }
}