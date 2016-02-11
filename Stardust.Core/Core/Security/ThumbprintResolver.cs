using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Core.Security
{
    public class ThumbprintResolver
    {
        public static Timer RefreshTime { get; private set; }

        private static bool thumbprintExtractionFailed;

        private static readonly ConcurrentBag<ServiceConfiguration> CachedIssuers = new ConcurrentBag<ServiceConfiguration>();

        private static IdentityConfiguration PassiveIdentityConfiguration;

        public static void RegisterServiceHost(ServiceConfiguration serviceHost)
        {
            if (CachedIssuers.Contains(serviceHost)) return;
            CachedIssuers.Add(serviceHost);
        }

        public static void RegisterWeb(IdentityConfiguration config)
        {
            PassiveIdentityConfiguration = config;
        }

        private static void RefreshThumbprintsTokens(object state)
        {
            if (PassiveIdentityConfiguration.IsInstance())
            {
                try
                {
                    var issuer = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.Issuer;
                    var thumbprint = GetThumbprintFromMetadata(issuer);
                    if (!thumbprint.IsInstance())
                    {
                        UpdateWebIdentity(issuer, thumbprint.Thumbprint);
                        CacheIssuerThumbprint(thumbprint, issuer);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, "Unable to update passive federation settings");
                }
            }
            foreach (var serviceHostBase in CachedIssuers)
            {
                try
                {
                    UpdateServiceHost(serviceHostBase);
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, "Unable to update service host");
                }
            }
        }

        private static void UpdateWebIdentity(string issuer, string thumbprint)
        {
            var issuers = PassiveIdentityConfiguration.IssuerNameRegistry as ConfigurationBasedIssuerNameRegistry;
            if (issuer.IsInstance())
            {
                if (!issuers.ConfiguredTrustedIssuers.ContainsKey(thumbprint))
                    issuers.AddTrustedIssuer(thumbprint, issuer);
            }
        }

        private static void UpdateServiceHost(ServiceConfiguration serviceHostBase)
        {
            var binding = serviceHostBase.Description.Endpoints.First().Binding as WS2007FederationHttpBinding;
            var issuer = binding.Security.Message.IssuerAddress.ToString();
            var thumbprint = GetThumbprintFromMetadata(issuer);
            if (!thumbprint.IsInstance())
            {
                var issuers = serviceHostBase.IdentityConfiguration.IssuerNameRegistry as ConfigurationBasedIssuerNameRegistry;
                if (issuer.IsInstance())
                {
                    if (!issuers.ConfiguredTrustedIssuers.ContainsKey(thumbprint.Thumbprint))
                    {
                        issuers.AddTrustedIssuer(thumbprint.Thumbprint, issuer);
                        CacheIssuerThumbprint(thumbprint, issuer);
                    }
                }
            }
        }

        static ThumbprintResolver()
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.LiveSingingTokeThumbprint") == "false") return;
            //RefreshTime = new Timer(RefreshThumbprintsTokens, null, new TimeSpan(0), new TimeSpan(0, 0, 10, 0));
        }

        /// <summary>
        /// Set the cache implementation to use. Inject your own if needed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetThumbprintCache<T>() where T : IThumbprintCache
        {
            Resolver.GetConfigurator().UnBind<IThumbprintCache>().AllAndBind().To<T>().SetTransientScope();
        }

        /// <summary>
        /// Caches the thumbprints localy
        /// </summary>
        public static void UseInprocCache()
        {
            SetThumbprintCache<InProcThumbprintCache>();
        }

        /// <summary>
        /// Caches the thumbprints in a cache server. You need to configure the service in your config set.
        /// </summary>
        /// <remarks>Implement IThumbprintCacheService and configure the service acordingly</remarks>
        public static void UseCacheServer()
        {
            SetThumbprintCache<ThumbprintCachService>();
        }

        private static IThumbprintCache Cache
        {
            get
            {
                return Resolver.Activate<IThumbprintCache>();
            }
        }

        public static string ResolveThumbprint(string thumbprint, string issuerAddress)
        {
            if (ConfigurationManagerHelper.GetValueOnKey("stardust.LiveSingingTokenThumbprint") == "false")
                return thumbprint;
            var thumbprintCahced = GetCachedThumbprint(issuerAddress);
            if (thumbprintCahced.ContainsCharacters()) return thumbprintCahced;
            var cert = GetThumbprintFromMetadata(issuerAddress);
            if (cert.IsInstance())
            {
                thumbprint = cert.Thumbprint;
                CacheIssuerThumbprint(cert, issuerAddress);
            }
            else
            {
                CacheIssuerThumbprint(thumbprint, issuerAddress);
            }
            return thumbprint;
        }

        private static X509Certificate2 GetThumbprintFromMetadata(string issuerAddress)
        {
            if (thumbprintExtractionFailed) return null;
            var issuerAdr = new Uri(issuerAddress);
            var metadataLocation = String.Format("{0}://{1}/federationmetadata/2007-06/federationmetadata.xml", issuerAdr.Scheme,
                issuerAdr.Host);
            using (var client = new WebClient())
            {
                var data = GetMetadataFromFederationServer(issuerAddress, client, metadataLocation);
                if (data.IsNullOrWhiteSpace())
                {
                    thumbprintExtractionFailed = true;
                    return null;
                }
                try
                {
                    var metadata = GetMetadatFromString(data);
                    return GetAndAddThumbprint(metadata, issuerAddress);
                }
                catch (Exception ex)
                {
                    thumbprintExtractionFailed = true;
                    Logging.DebugMessage("------------------------------------------------------------------------");
                    Logging.DebugMessage(data);
                    Logging.DebugMessage("------------------------------------------------------------------------");
                    Logging.Exception(ex, "Uable to extract thumbprint from metadata");
                    return null;
                }
            }
        }

        private static void CacheIssuerThumbprint(string thumbprint, string issuerAddress)
        {
            Cache.CacheThumbprint(new ThumbprintItem { Expiry = DateTime.Now.AddDays(1), Resolved = true, Thumbprint = thumbprint }, issuerAddress);
        }

        private static string GetCachedThumbprint(string issuerAddress)
        {
            var cachedItem = Cache.GetChacedThumbprint(issuerAddress);
            return cachedItem.IsInstance() ? cachedItem.Thumbprint : null;
        }

        private static X509Certificate2 GetAndAddThumbprint(IEnumerable<X509Certificate2> metadata, string issuerAddress)
        {
            var thumbPrintFromCert = metadata.FirstOrDefault();
            if (!thumbPrintFromCert.IsInstance()) return null;
            CacheIssuerThumbprint(thumbPrintFromCert, issuerAddress);
            return thumbPrintFromCert;
        }

        private static void CacheIssuerThumbprint(X509Certificate2 thumbPrintFromCert, string issuerAddress)
        {
            Cache.CacheThumbprint(new ThumbprintItem { Expiry = thumbPrintFromCert.NotAfter, Resolved = true, Thumbprint = thumbPrintFromCert.Thumbprint }, issuerAddress);
        }

        private static string GetMetadataFromFederationServer(string issuerAddress, WebClient client, string metadataLocation)
        {
            var data = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
            try
            {
                data += client.DownloadString(metadataLocation);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, string.Format("Unable to download federation metadata for {0}", issuerAddress));
            }
            return data;
        }

        private static IEnumerable<X509Certificate2> GetMetadatFromString(string data)
        {
            var document = new XmlDocument();
            document.LoadXml(data);
            XmlNode root = document.DocumentElement;
            var NS = new XmlNamespaceManager(document.NameTable);
            NS.AddNamespace("default", "urn:oasis:names:tc:SAML:2.0:metadata");
            NS.AddNamespace("keys", "http://www.w3.org/2000/09/xmldsig");
            NS.AddNamespace("keys1", "http://www.w3.org/2000/09/xmldsig#");
            var cert = root.SelectNodes("descendant::keys1:X509Certificate", NS);
            foreach (var thisNode in cert.Cast<XmlNode>().Where(thisNode => thisNode.ParentNode.ParentNode.ParentNode.Name == "Signature"))
            {
                var thisText = thisNode.InnerText;
                var keydata = Convert.FromBase64String(thisText);
                var x509C = new X509Certificate2(keydata);
                yield return x509C;
            }
        }

    }
}