using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using Stardust.Core;
using Stardust.Core.Default.Implementations;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;
using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace AuthenticationTestSite
{
    public class TestBlueprint : Blueprint
    {
        protected sealed override void SetDefaultBindings()
        {
            Configurator.Bind<IAppPoolRecycler>().To<AppPoolRecycler>().SetSingletonScope();
            //Setting default JsonSettings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                                                    {
                                                        NullValueHandling = NullValueHandling.Ignore,
                                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                        Culture = CultureInfo.InvariantCulture,

                                                    };
            Configurator.Bind<IConfigurationReader>().To<StarterkitConfigurationReader>();
            Configurator.Bind<IReplaceableSerializer>().To<ProtobufReplaceableSerializer>().SetSingletonScope();
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidation;
            Bind<IUrlFormater, UrlFormater>().SetSingletonScope().AllowOverride = false;
            Configurator.Bind<IServiceTearDown>().To<DefaultServiceTearDown>().SetSingletonScope();
            Configurator.Bind<IStardustWebInitializer>().To<WebInitializer>().SetTransientScope();
        }

        /// <summary>
        /// Place your bindings here
        /// </summary>
        protected override void DoCustomBindings()
        {
        }
        private bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None || certificate.Subject.Contains("terstest1-vm1.cloudapp.net");
        }
    }

    public class AppPoolRecycler : IAppPoolRecycler
    {
        public bool TryRecycleCurrent()
        {
            if (!HostingEnvironment.IsHosted||HostingEnvironment.IsDevelopmentEnvironment)
            {
                Logging.DebugMessage("Unloading app domain in non hosted mode.");
                Thread.Sleep(1000);
                HttpRuntime.UnloadAppDomain();
                return true;
            }
            using (var manager = new ServerManager())
            {
                try
                {
                    var site = manager.Sites.SingleOrDefault(s => s.Name == HostingEnvironment.ApplicationHost.GetSiteName());
                    if (site == null) return false;
                    var app = site.Applications.SingleOrDefault(a => a.Path == HostingEnvironment.ApplicationVirtualPath);
                    if (app == null) return false;
                    var appPool = manager.ApplicationPools.SingleOrDefault(pool => pool.Name == app.ApplicationPoolName);
                    if (appPool == null) return false;
                    var state = appPool.Recycle();
                    Logging.DebugMessage("Pool recycled. New pool state is {0}", state);
                    return true;
                }
                catch (NotImplementedException )
                {
                    Logging.DebugMessage("Unloading app domain in IIS express.");
                    Thread.Sleep(1000);
                    HttpRuntime.UnloadAppDomain();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return false;
                }

            }
        }

        public bool TryRecycle(string appPoolId)
        {
            return false;
        }

        public bool TryRecycleAll()
        {
            using (var manager = new ServerManager())
            {
                try
                {
                    foreach (var appPool in manager.ApplicationPools)
                    {
                        var state = appPool.Recycle();
                        Logging.DebugMessage("Pool {1} recycled. New pool state is {0}", state, appPool.Name);
                    }
                    return true;
                }
                catch (System.Exception ex)
                {
                    ex.Log();
                    return false;
                }

            }
        }
    }
}