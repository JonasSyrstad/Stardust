using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Stardust.Core.Default.Implementations;
using Stardust.Core.Service.Web;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;

namespace Stardust.Starterkit.Proxy.App_Start
{
    public class ProxyBlupeprint:Blueprint
    {
        protected override void DoCustomBindings()
        {
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidation;
            Configurator.Bind<IReplaceableSerializer>().To<JsonReplaceableSerializer>().SetSingletonScope();
            Configurator.Bind<IConfigurationReader>().To<StarterkitConfigurationReader>().SetSingletonScope();//update this to add caching and other features not supported in the standard
           //override the default bindings here....
            base.DoCustomBindings();
        }

        private bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None || certificate.Subject.Contains("terstest1-vm1.cloudapp.net");
        }
    }
}