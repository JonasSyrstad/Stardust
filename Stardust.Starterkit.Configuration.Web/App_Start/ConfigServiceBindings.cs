using System;
using System.Diagnostics;
using Keen.Core;
using Stardust.Core;
using Stardust.Interstellar;
using Stardust.Interstellar.Endpoints;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Business.CahceManagement;
using Stardust.Starterkit.Configuration.Repository;
using Environment = System.Environment;

namespace Stardust.Starterkit.Configuration.Web
{
    public class ConfigServiceBindings : Blueprint<KeenLogger>
    {
        protected override void DoCustomBindings()
        {
            Resolver.Bind<IBindingCreator>().To<BasicBindingCreator>("oracle").SetThreadScope();
            Resolver.Bind<IConfigSetTask>().To<ConfigSetTask>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<IEnvironmentTasks>().To<EnvironmentTasks>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<IRepositoryFactory>().To<RepositoryFactory>().SetSingletonScope().AllowOverride = false;
            Resolver.Bind<IUserFacade>().To<UserFacade>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<ICacheManagementService>().To<CacheManagementService>().SetTransientScope();
            Resolver.Bind<ICacheManagementWrapper>().To<NullCacheManagementWrapper>().SetTransientScope();
            Resolver.Bind<ICacheManagementWrapper>().To<AzureRedisFabricCacheManager>("Azure").SetTransientScope();
        }
    }

    public class KeenLogger : ILogging
    {
        private LoggingDefaultImplementation Logger;
        private KeenClient keenClient;

        private KeenClient keenReadClient;

        private static string GetProjectId()
        {
            return ConfigurationManagerHelper.GetValueOnKey("keen.io.project.id");
        }

        private static string GetProjectKey()
        {
            return ConfigurationManagerHelper.GetValueOnKey("keen.io.project.key");
        }

        private static string GetProjectReadKey()
        {
            return ConfigurationManagerHelper.GetValueOnKey("keen.io.project.key.read");
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public KeenLogger()
        {
            Logger = new LoggingDefaultImplementation();
            try
            {
                if (keenClient != null) return;
                var prjSettings = new ProjectSettingsProvider(GetProjectId(), writeKey: GetProjectKey());
                keenClient = new KeenClient(prjSettings);
                keenClient.AddGlobalProperty("serviceHost", Utilities.GetServiceName());
                keenClient.AddGlobalProperty("environment", "Config");
                keenClient.AddGlobalProperty("configSet", "ALL");
                keenClient.AddGlobalProperty("machine", Environment.MachineName);
                keenClient.AddGlobalProperty("datacenterKey", "cnfwe");
                var prjReadSettings = new ProjectSettingsProvider(GetProjectId(), readKey: GetProjectReadKey());
                keenReadClient = new KeenClient(prjReadSettings);
            }
            catch (Exception)
            {
            }
        }

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            Logger.Exception(exceptionToLog, additionalDebugInformation);
            Error(exceptionToLog, additionalDebugInformation);
        }

        public void HeartBeat()
        {
            Logger.HeartBeat();
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information, string additionalDebugInformation = null)
        {
            Logger.DebugMessage(message, entryType, additionalDebugInformation);
            Debug(message,entryType,additionalDebugInformation);
        }

        public void SetCommonProperties(string logName)
        {
            Logger.SetCommonProperties(logName);
        }
        public void Debug(string message, EventLogEntryType entryType, string additionalDebugInformation)
        {
            try
            {

                keenClient.AddEventAsync(string.Format("{0}.Debug.Details", "Config"), new
                {
                    Message = message,
                    DebugMessage = additionalDebugInformation,
                    entryType,
                    TimeStamp = DateTime.UtcNow,
                });

            }
            catch (Exception ex)
            {
                // ex.Log();
            }
        }

        public void Error(Exception exception, string additionalDebugInformation)
        {
            try
            {
                keenClient.AddEventAsync(string.Format("{0}.Error.Details", "Config"), new
                {
                    exception.Message,
                    exception.StackTrace,
                    DebugMessage = additionalDebugInformation,
                    TimeStamp = DateTime.UtcNow,
                });

            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public static string LogContainerName
        {
            get
            {
                return string.Format("{1}{0}", Utilities.GetEnvironment(), "Config");
            }
        }
    }
}