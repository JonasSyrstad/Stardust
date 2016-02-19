using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Frontier.BatchProcessor.Default
{
    public class DefaultKernel<T> : IBatchCoordinatorKernel<T> where T : class
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> propertyInfoCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private IKernelRepository Repository;
        private EnvironmentDefinition Environment;
        private static ConcurrentDictionary<string, bool> ConnectorState = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, IBatchConnector<T>> Connectors = new ConcurrentDictionary<string, IBatchConnector<T>>();
        private IBatchProgressLogger Logger;

        public DefaultKernel(IKernelRepository repository)
        {
            Repository = repository;
        }

        public bool UpdateItem(string itemId, T withPayload, string source)
        {
            var oldItem = GetItem(itemId, source);
            if (IsNewItem(withPayload, oldItem))
                return Repository.UpsertItem(itemId, withPayload, UpdateTypes.Add, source);
            if (IsDeleteItem(withPayload, oldItem))
                return Repository.UpsertItem(itemId, withPayload, UpdateTypes.Delete, source);
            ValidateItemType(withPayload, oldItem);
            var changeType = AssertChangeType(withPayload, oldItem);
            return Repository.UpsertItem(itemId, withPayload, changeType, source);
        }

        private static void ValidateItemType(T withPayload, object oldItem)
        {
            if (oldItem.GetType() != withPayload.GetType())
                throw new InvalidCastException("Cannot update item with different type from the existion one");
        }

        private static bool IsDeleteItem(T withPayload, object oldItem)
        {
            return oldItem.IsInstance() && withPayload.IsNull();
        }

        private static bool IsNewItem(T withPayload, object oldItem)
        {
            return oldItem.IsNull() && withPayload.IsInstance();
        }

        private static UpdateTypes AssertChangeType(object withPayload, object oldItem)
        {
            var properties = GetPropertiesToCompare(withPayload);
            var changeType = UpdateTypes.Unchanged;
            foreach (var propertyInfo in properties)
            {
                var oldValue = propertyInfo.GetGetMethod().Invoke(oldItem, null);
                var newValue = propertyInfo.GetGetMethod().Invoke(withPayload, null);
                if (newValue == oldValue) continue;
                changeType = UpdateTypes.Update;
                break;
            }
            return changeType;
        }

        private static IEnumerable<PropertyInfo> GetPropertiesToCompare(object withPayload)
        {
            PropertyInfo[] properties;
            if (propertyInfoCache.TryGetValue(withPayload.GetType(), out properties)) return properties;
            properties = withPayload.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            propertyInfoCache.TryAdd(withPayload.GetType(), properties);
            return properties;
        }

        public T GetItem(string itemId, string source)
        {
            var coordinatedItem = Repository.GetItemById(itemId);
            if (source.IsNullOrWhiteSpace()) return coordinatedItem.MasterItem as T;
            if (coordinatedItem.IsNull()) return default(T);
            object item;
            if (!coordinatedItem.Sources.TryGetValue(source, out item))
                return default(T);
            return (T)item;
        }

        public bool InvalidateUpdateFlag(string itemId, string source)
        {
            var item = Repository.GetItemById(itemId);
            return Repository.UpsertItem(itemId, item, UpdateTypes.ExportError, source);
        }

        public IEnumerable<string> GetModifiedItems(DateTime since)
        {
            return Repository.GetModifiedItems(since);
        }

        public bool IsSynchronizerRunning(string name)
        {
            bool running;
            if (!ConnectorState.TryGetValue(name, out running))
                ConnectorState.TryAdd(name, false);
            return running;
        }

        public void SetExportItem(string itemId, ICoordinatedItemContainer coordinationItem)
        {
            Repository.UpsertItem(itemId, coordinationItem);
        }

        public void SetSynchronizationRunState(bool value, string name)
        {
            ConnectorState.AddOrUpdate(name, value);
        }

        public IBatchCoordinatorKernel<T> Initialize(EnvironmentDefinition environment)
        {
            Environment = environment;
            Repository.SetEnvironment(environment.Name);
            Logger = Resolver.Activate<IBatchProgressLogger>().SetEnvironment(Environment);
            return this;
        }



        public IBatchConnector<T> GetConnector(string name)
        {
            IBatchConnector<T> connector;
            if (!Connectors.TryGetValue(name, out connector))
            {
                connector = Resolver.Activate<IBatchConnector<T>>(name,t=>t.Initialize(this, Environment).SetLogger(Logger));
                if (Connectors.TryAdd(name, connector)) return connector;
                if (!Connectors.TryGetValue(name, out connector)) throw new ArgumentException("Unable to get connector");
            }
            return connector;
        }

        public IEnumerable<string> GetConnectors(ConnectorTypes ofType)
        {
            return (from i in Resolver.GetImplementationsOf<IBatchConnector<T>>()
                    where Resolver.Activate<IBatchConnector<T>>(i.Key).Type == ofType
                    select i.Key).ToList();
        }

        public IDictionary<string, ConnectorTypes> GetConnectors()
        {
            return (from i in Resolver.GetImplementationsOf<IBatchConnector<T>>()
                    let instance = Resolver.Activate<IBatchConnector<T>>(i.Key)
                    select new { i.Key, Value = instance.Type }).ToDictionary(k => k.Key, v => v.Value);
        }

        public IBatchProgressLogger GetLogger()
        {
            return Logger;
        }

        public bool TerminateAllConnectors()
        {
            var connectors = GetConnectors();
            foreach (var batchConnector in connectors)
            {
                if (IsSynchronizerRunning(batchConnector.Key))
                    GetConnector(batchConnector.Key).Terminate();
            }
            var stillRunning = true;
            while (stillRunning)
            {
                stillRunning = false;
                foreach (var batchConnector in connectors)
                {
                    if (IsSynchronizerRunning(batchConnector.Key))
                        stillRunning = true;
                }
                Thread.Sleep(100);
            }
            return true;
        }

        public bool TerminateConnector(string name)
        {
            if (!IsSynchronizerRunning(name))
                return true;
            TerminateConnector(name);
            while (true)
            {
                if (!IsSynchronizerRunning(name)) return true;
                Thread.Sleep(100);
            }
        }


        public ICoordinatedItemContainer GetItem(string itemId)
        {
            return Repository.GetItemById(itemId);
        }
    }
}
