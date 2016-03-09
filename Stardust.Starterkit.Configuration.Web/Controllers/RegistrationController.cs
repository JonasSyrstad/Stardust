using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Stardust.Interstellar;
using Stardust.Particles;
using Stardust.Particles.Xml;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Stardust.Starterkit.Configuration.Web.Models;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {

        private readonly IEnvironmentTasks environmentTasks;

        private readonly IConfigSetTask configSetTasks;

        private string id;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Mvc.Controller"/> class.
        /// </summary>
        public RegistrationController(IEnvironmentTasks environmentTasks, IConfigSetTask configSetTasks
            )
        {
            this.environmentTasks = environmentTasks;
            this.configSetTasks = configSetTasks;
        }

        [HttpPost]
        public ActionResult TryAddService(string serviceMetadata)
        {
            try
            {
                if (serviceMetadata.IsNullOrWhiteSpace())
                {
                    using (var tr = new StreamReader(Request.InputStream))
                    {
                        serviceMetadata = tr.ReadToEnd();
                    }
                }
                var data = Deserializer<ServiceRegistrationServer.ServiceRegistrationMessage>.Deserialize(serviceMetadata);
                
                var cs = configSetTasks.GetConfigSet(data.ConfigSetId);
                var service = (from s in cs.Services where s.Name == data.ServiceName select s).SingleOrDefault();
                if (service.IsNull())
                {
                    service = configSetTasks.CreateService(cs, data.ServiceName);
                    configSetTasks.CreateEndpoint(service, data.DefaultBinding, data.Properties);
                    var env = (from e in cs.Environments where e.Name == data.Environment select e).SingleOrDefault();
                    if (env.IsNull())
                    {
                        env = environmentTasks.CreatEnvironment(cs, data.Environment);
                    }
                    var serviceRoot = (from sp in env.SubstitutionParameters where sp.Name == string.Format("{0}_Address", data.ServiceName) select sp).SingleOrDefault();
                    if (serviceRoot.IsNull() && data.DefaultEnvirionmentUrlPath.ContainsCharacters())
                        serviceRoot.ItemValue = data.DefaultEnvirionmentUrlPath;
                    ConnectToHost(data, cs, service);

                }
                return Json("OK");
            }
            catch (Exception exception)
            {
                Logging.Exception(exception);
                return Json(string.Format("Error: {0}", exception.Message));
            }
        }

        private  void ConnectToHost(ServiceRegistrationServer.ServiceRegistrationMessage data, IConfigSet cs, IServiceDescription service)
        {
            if (data.ServiceHost != null)
            {
                var host = cs.ServiceHosts.SingleOrDefault(s => s.Name == data.ServiceHost);
                if (host != null)
                {
                    service.ServiceHost = host;
                    service.ServiceHostId = host.Id;
                    configSetTasks.UpdateService(service);
                }
            }
        }

        [HttpPost]
        public ActionResult InitializeDataCenter(string id, string registration)
        {
            this.id = id;
            try
            {
                var settings = GetData<ConfigurationSettings>(registration);

                settings = configSetTasks.InitializeDatacenter(id, settings);
                return Json(settings);
            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult PublishDataCenter(string id, string registration)
        {
            this.id = id;
            try
            {
                var settings = GetData<ConfigurationSettings>(registration);
                settings = configSetTasks.PublishDatacenter(id, settings);
                return Json(settings);
            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult InitializeEnvironment(string id, string registration)
        {
            this.id = id;
            try
            {
                var settings = GetData<FederationSettings>(registration);
                environmentTasks.ConfigureEnvironment(id, settings);

            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(500, ex.Message);
            }
            return new HttpStatusCodeResult(200, "OK");
        }

        [HttpPost]
        public ActionResult DeleteEnvironment(string id, string registration)
        {
            this.id = id;
            try
            {
                var settings = GetData<string>(registration);
                var env = environmentTasks.GetEnvironment(string.Format("{0}-{1}", id, settings));
               if (env == null) return new HttpStatusCodeResult(HttpStatusCode.Gone, "OK");
               environmentTasks.DeleteEnvironment(env);

            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(500, ex.Message);
            }
            return new HttpStatusCodeResult(200, "OK");
        }

        private T GetData<T>(string registration)
        {
            if (registration.IsNullOrWhiteSpace())
            {
                using (var tr = new StreamReader(Request.InputStream))
                {
                    registration = tr.ReadToEnd();
                }
            }

            var settings = JsonConvert.DeserializeObject<T>(registration);
            return settings;
        }

        [HttpPost]
        public ActionResult SetProperty(string id, string properyMessage)
        {
            this.id = id;
            try
            {
                var settings = GetData<PropertyRequest>(properyMessage);
                var env = environmentTasks.GetEnvironment(string.Format("{0}-{1}", id, settings.Environment));
                switch (settings.Type)
                {
                    case VariableTypes.Environmental:
                        SetEnvironmentVariable( env, settings.PropertyName, settings.Value, settings.IsSecure);
                        break;
                    case VariableTypes.ServiceHost:
                    case VariableTypes.ServiceHostEnvironmental:
                        var host = configSetTasks.GetServiceHost(string.Format("{0}-{1}", id, settings.ParentContainer));
                        SetHostParameter(configSetTasks, settings, host);
                        if (settings.Type == VariableTypes.ServiceHostEnvironmental)
                        {
                            var envKey = GetEnvironmentSubstitutionKey(settings);
                            SetEnvironmentSubstitutionVariable( env, envKey, settings.Value, settings.IsSecure);
                        }
                        break;
                    case VariableTypes.Service:
                    case VariableTypes.ServiceEnvironmental:
                        SetEnpointParameter( settings);
                        if (settings.Type == VariableTypes.ServiceHostEnvironmental)
                        {
                            var envKey = GetEnvironmentSubstitutionKey(settings);
                            SetEnvironmentSubstitutionVariable( env, envKey, settings.Value, settings.IsSecure);
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                ex.Log();
                return new HttpStatusCodeResult(500, ex.Message);
            }
            return new HttpStatusCodeResult(200, "OK");
        }

        private void SetEnpointParameter(PropertyRequest settings)
        {
            var endpoint = configSetTasks.GetEndpoint(string.Format("{0}-{1}-{2}", id, settings.ParentContainer, settings.SubContainer));
            var item = endpoint.Parameters.SingleOrDefault(p => p.Name == settings.PropertyName);
            if (item == null)
            {
                configSetTasks.CreateEndpointParameter(
                    settings.ParentContainer,
                    settings.PropertyName,
                    settings.Type == VariableTypes.ServiceHostEnvironmental ? settings.ParentFormatString : settings.Value,
                    settings.Type == VariableTypes.ServiceHostEnvironmental);
            }
            else
            {

                item.ConfigurableForEachEnvironment = settings.Type == VariableTypes.ServiceHostEnvironmental;
                item.ItemValue =
                    settings.Type == VariableTypes.ServiceHostEnvironmental ? settings.ParentFormatString : settings.Value;
                configSetTasks.UpdateEndpointParameter(item);
            }
        }

        private string GetEnpointParameterKey(PropertyRequest settings)
        {
            return string.Format("{0}-{1}-{2}-{3}", id, settings.ParentContainer, settings.SubContainer, settings.PropertyName);
        }

        private string GetEnpointKey(PropertyRequest settings)
        {
            return string.Format("{0}-{1}-{2}", id, settings.ParentContainer, settings.SubContainer, settings.PropertyName);
        }

        private  void SetHostParameter(IConfigSetTask reader, PropertyRequest settings, IServiceHostSettings host)
        {
            var item = host.Parameters.SingleOrDefault(p => p.Name == settings.PropertyName);
            if (item == null)
            {
                reader.CreateServiceHostParameter(
                    host,
                    settings.PropertyName,
                    settings.IsSecure,
                    settings.Type == VariableTypes.ServiceHostEnvironmental ? settings.ParentFormatString : settings.Value,
                    settings.Type == VariableTypes.ServiceHostEnvironmental);
            }
            else
            {
                item.IsSecureString = settings.IsSecure;
                item.IsEnvironmental = settings.Type == VariableTypes.ServiceHostEnvironmental;
                item.SetValue(
                    settings.Type == VariableTypes.ServiceHostEnvironmental ? settings.ParentFormatString : settings.Value);
                reader.UpdateHostParameter(item);
            }
        }

        private void SetEnvironmentVariable(IEnvironment env, string name, string value, bool isSecure)
        {
            var item = env.EnvironmentParameters.SingleOrDefault(p=>p.Name==name);
            if (item == null)
                environmentTasks.CreatEnvironmentParameter(env, name, value, isSecure);
            else
            {
                item.IsSecureString = isSecure;
                item.SetValue(value);
                environmentTasks.UpdateEnvironmentParameter(item);
            }
        }


        private void SetEnvironmentSubstitutionVariable( IEnvironment env, string name, string value, bool isSecure)
        {
            var item = env.SubstitutionParameters.SingleOrDefault(p => p.Name == name);
            if (item == null) environmentTasks.CreateSubstitutionParameter(env, name, value);
            else
            {
                item.ItemValue = value;
                environmentTasks.UpdateSubstitutionParameter(item);
            }
        }
        private  string GetEnvironmentSubstitutionKey(PropertyRequest settings)
        {
            return string.Format("{0}_{1}", settings.ParentContainer, settings.PropertyName);
        }

        [HttpPut]
        public ActionResult Publish(string id,string environment)
        {
            try
            {
                environment = GetData<string>(environment);
                Logging.DebugMessage(environment);
                this.id = id;
                var cs = configSetTasks.GetConfigSet(id);
                var env = cs.Environments.Single(e => e.Name.Equals(environment, StringComparison.OrdinalIgnoreCase));
                var result = environmentTasks.PushChange(cs.Id, environment);
                return Json(new { Status = result ? "OK" : "Failure", Set = cs.Id, Environment = environment, env.ETag, UpdateDate = env.LastPublish });
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }
    }


}