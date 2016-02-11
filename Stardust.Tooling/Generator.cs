using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Stardust.Accretion;

namespace Stardust.Stardust_Tooling
{
    public class Rootobject
    {
        public int Id { get; set; }
        public string SetName { get; set; }
        public string ParentSet { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Created { get; set; }
        public object Parameters { get; set; }
        public Service[] Services { get; set; }
        public Environment[] Environments { get; set; }
        public Endpoint[] Endpoints { get; set; }
        public string RequestedBy { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public object ConnectionStrings { get; set; }
        public Parameter[] Parameters { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public bool PasswordIsUnchanged { get; set; }
        public Identitysettings IdentitySettings { get; set; }
    }

    public class Identitysettings
    {
        public string CertificateValidationMode { get; set; }
        public string IssuerName { get; set; }
        public string Thumbprint { get; set; }
        public string[] Audiences { get; set; }
        public string IssuerAddress { get; set; }
        public string Realm { get; set; }
        public bool RequireHttps { get; set; }
        public bool EnforceCertificateValidation { get; set; }
    }

    public class Parameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public object[] ChildParameters { get; set; }
        public bool HasChildren { get; set; }
        public object Set { get; set; }
        public object Environment { get; set; }
        public string BinaryValue { get; set; }
    }

    public class Environment
    {
        public int Id { get; set; }
        public string EnvironmentName { get; set; }
        public Parameter1[] Parameters { get; set; }
        public object Set { get; set; }
        public Cache Cache { get; set; }
    }

    public class Cache
    {
        public string CacheType { get; set; }
        public string CacheImplementation { get; set; }
        public bool NotifyOnChange { get; set; }
        public string CacheName { get; set; }
        public string MachineNames { get; set; }
        public int Port { get; set; }
        public bool Secure { get; set; }
        public string PassPhrase { get; set; }
        public object SecurityMode { get; set; }
        public object ProtectionLevel { get; set; }
    }

    public class Parameter1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public object[] ChildParameters { get; set; }
        public bool HasChildren { get; set; }
        public object Set { get; set; }
        public object Environment { get; set; }
        public string BinaryValue { get; set; }
    }

    public class Endpoint
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string ActiveEndpoint { get; set; }
        public Endpoint1[] Endpoints { get; set; }
        public object Set { get; set; }
        public bool Deleted { get; set; }
    }

    public class Endpoint1
    {
        public int Id { get; set; }
        public string EndpointName { get; set; }
        public string BindingType { get; set; }
        public string MessageFormat { get; set; }
        public int MaxMessageSize { get; set; }
        public string HostNameComparisonMode { get; set; }
        public string TextEncoding { get; set; }
        public string Address { get; set; }
        public object Parent { get; set; }
        public string MaxConnections { get; set; }
        public string Durable { get; set; }
        public int MaxReceivedSize { get; set; }
        public int MaxBufferPoolSize { get; set; }
        public int MaxBufferSize { get; set; }
        public Readerquotas ReaderQuotas { get; set; }
        public int TransferMode { get; set; }
        public bool ExactlyOnce { get; set; }
        public int CloseTimeout { get; set; }
        public int OpenTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int SendTimeout { get; set; }
        public bool Deleted { get; set; }
        public string IssuerName { get; set; }
        public string IssuerActAsAddress { get; set; }
        public string IssuerMetadataAddress { get; set; }
        public string Audience { get; set; }
        public string Thumbprint { get; set; }
        public string CertificateValidationMode { get; set; }
        public bool OverrideSslSecurity { get; set; }
        public string IssuerAddress { get; set; }
        public string StsAddress { get; set; }
        public bool Ignore { get; set; }
        public Dictionary<string, string> PropertyBag { get; set; }
    }

    public class Readerquotas
    {
        public int MaxStringContentLength { get; set; }
        public int MaxArrayLength { get; set; }
        public int MaxBytesPerRead { get; set; }
        public int MaxDepth { get; set; }
        public int MaxNameTableCharCount { get; set; }
        public int ModifiedQuotas { get; set; }
    }



    public class Generator
    {
        public ICodeFileWriterContext CreateFile(string @namespace, string content)
        {
            var writer = WriterFactory.CreateWriter();
            writer.AddGeneratorHeader()
                .Using("System")
                .Using("System.Linq")
                .Using("Newtonsoft.Json")
                .Using("Stardust.Interstellar")
                .Using("Stardust.Particles")
                .DeclareNamespace(@namespace,
                    w =>
                        {
                            w.StaticClass(
                                "ConfigurationExtensions",
                                cw =>
                                    {
                                        cw.ExtMethod("ConfigurationSettings","GetConfiguration","IRuntime",
                                            mw =>
                                                {
                                                    mw.Return("ConfigurationSettings.Current");
                                                });
                                        cw.ExtMethod("AppSettings", "AppSettings", "IRuntime",
                                            mw =>
                                            {
                                                mw.Return("ConfigurationSettings.Current.AppSettings");
                                            });
                                        cw.ExtMethod("Environment", "Environment", "IRuntime",
                                            mw =>
                                            {
                                                mw.Return("ConfigurationSettings.Current.Environment");
                                            });
                                        cw.ExtMethod("ServiceHosts", "ServiceHosts", "IRuntime",
                                            mw =>
                                            {
                                                mw.Return("ConfigurationSettings.Current.ServiceHosts");
                                            });
                                    });
                        w.Class(
                            "ConfigurationSettings",
                            cw =>
                                {
                                    cw.Field("static ConfigurationSettings","_current",null);
                                    cw.Property("static ConfigurationSettings", "Current",
                                        gw =>
                                            {
                                                gw.If("_current==null", bw => { bw.AssignVariable("_current", "new ConfigurationSettings()"); });
                                                gw.Return("_current");
                                            },null);
                                cw.Field("AppSettings", "appSettings", "new AppSettings()");
                                cw.Property(
                                    "AppSettings",
                                    "AppSettings",
                                    gw =>
                                    {
                                        gw.Return("appSettings");
                                    }, null);
                                cw.Field("Environment", "environment", "new Environment()");
                                cw.Property(
                                    "Environment",
                                    "Environment",
                                    gw =>
                                    {
                                        gw.Return("environment");
                                    }, null);
                                cw.Field("ServiceHosts", "serviceHosts", "new ServiceHosts()");
                                cw.Property(
                                    "ServiceHosts",
                                    "ServiceHosts",
                                    gw =>
                                    {
                                        gw.Return("serviceHosts");
                                    }, null);
                            });
                        w.Class("AppSettings",
                            cw =>
                            {
                                cw.InternalCtor();
                                cw.Property("string", "ConfigSetName",
                                    gw =>
                                    {
                                        gw.Return(gw.GetValueOnKey("configSet"));
                                    }, null);
                                cw.Property("string", "Environment",
                                    gw =>
                                    {
                                        gw.Return(gw.GetValueOnKey("environment"));
                                    }, null);
                                cw.Property("string", "ServiceHostName",
                                    gw =>
                                    {
                                        gw.Return(gw.GetValueOnKey("serviceName"));
                                    }, null);
                                cw.Property("string", "datacenter",
                                    gw =>
                                    {
                                        gw.Var("value", gw.GetValueOnKey("dataCenterConfigKeyName"));
                                        gw.If("value.IsNullOrWhiteSpace()",
                                            bw =>
                                            {
                                                bw.Return(gw.GetValueOnKey("dataCenterName"));
                                            })
                                            .Else(bw => bw.Return("value"));

                                    }, null);
                                cw.Property("string", "ConfigurationServiceUrl",
                                    gw =>
                                    {
                                        gw.Return(gw.GetValueOnKey("stardust.configLocation"));
                                    }, null);
                            });
                        w.Class("Environment",
                            cw =>
                            {
                                var added = new Dictionary<string,bool>();
                                foreach (var propertyName in cw.GetEnvironments(content).GetParameters())
                                {

                                    var name = propertyName.Replace(".", "").Replace("-", "").Replace(",", "").Replace("_", "");
                                    if (added.ContainsKey(name.Trim()))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        added.Add(name.Trim(),true);
                                        var privateName = string.Format("_{0}", name);
                                        if (name.ToLower().Contains("password") || name.ToLower().Contains("key") || name.ToLower().Contains("accountname") || name.ToLower().Contains("username") || name.ToLower().Contains("user") || name.ToLower().Contains("account") || name.ToLower().Contains("secret") || name.ToLower().Contains("connectionstring") || name.ToLower().Contains("thumbprint"))
                                            cw.Attribute("JsonIgnore");
                                        cw.Property<string>(
                                            name,
                                            gw =>
                                                {
                                                    gw.Var(privateName, string.Format("RuntimeFactory.Current.Context.GetEnvironmentConfiguration().GetConfigParameter(\"{0}\")", propertyName));
                                                    gw.If("string.IsNullOrEmpty(" + privateName + ")", iw =>
                                                        {
                                                            iw.Return("string.Empty");
                                                        })
                                                        .Else(ew =>
                                                            {
                                                                ew.Return(privateName);
                                                            });
                                                }
                                            , null);
                                    }
                                }
                                foreach (var propertyName in cw.GetEnvironments(content).GetSecureParameters())
                                {
                                    var name = propertyName.Replace(".", "").Replace("-", "").Replace(",", "").Replace("_", "");
                                    if (added.ContainsKey(name.Trim()))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        added.Add(name.Trim(), true);
                                        var privateName = string.Format("_{0}", name);
                                        cw.Attribute("JsonIgnore");
                                        cw.Property<string>(
                                            name,
                                            gw =>
                                                {
                                                    gw.Var(privateName, string.Format("RuntimeFactory.Current.Context.GetEnvironmentConfiguration().GetSecureConfigParameter(\"{0}\")", propertyName));
                                                    gw.If(
                                                        "string.IsNullOrEmpty(" + privateName + ")",
                                                        iw =>
                                                            {
                                                                iw.Return("string.Empty");
                                                            }).Else(
                                                                ew =>
                                                                    {
                                                                        ew.Return(privateName);
                                                                    });
                                                },
                                            null);
                                    }
                                }
                            });
                        w.Class("ServiceHosts",
                            cw =>
                            {
                                foreach (var service in cw.GetConfig(content).Services)
                                {
                                    cw.Field(service.ServiceName + "ServiceHost", "_" + service.ServiceName.ToLower(), "new " + service.ServiceName + "ServiceHost()");
                                    cw.Property(
                                        service.ServiceName + "ServiceHost",
                                        service.ServiceName,
                                        gw =>
                                        {
                                            gw.Return("_" + service.ServiceName.ToLower());
                                        },
                                        null);
                                    cw.Class(
                                        service.ServiceName + "ServiceHost",
                                        ccw =>
                                        {
                                            foreach (var parameter in service.Parameters)
                                            {
                                                var name = parameter.Name.Trim().Replace(".", "").Replace("-", "").Replace(",", "").Replace("_", "");
                                                var privateName = string.Format("_{0}", name);
                                                cw.Property<string>(
                                                    name,
                                                    gw =>
                                                    {
                                                        gw.Var(privateName, string.Format("RuntimeFactory.Current.Context.GetServiceConfiguration(\"{1}\").GetConfigParameter(\"{0}\")", parameter.Name, service.ServiceName));
                                                        gw.If("string.IsNullOrEmpty(" + privateName + ")", iw =>
                                                        {
                                                            iw.Return("string.Empty");
                                                        })
                                                            .Else(ew =>
                                                            {
                                                                ew.Return(privateName);
                                                            });
                                                    }
                                                        , null);
                                            }
                                        });
                                }
                            });
                        
                    });
            return writer;
        }

        public int Save(ICodeFileWriterContext writer, IntPtr[] rgbOutputFileContents)
        {
            var bytes = Encoding.UTF8.GetBytes(writer.ToString());
            var length = bytes.Length;
            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(bytes, 0, rgbOutputFileContents[0], length);
            return length;
        }
    }
}