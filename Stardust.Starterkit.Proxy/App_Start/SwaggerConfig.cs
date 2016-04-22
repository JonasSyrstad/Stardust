using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;
using WebActivatorEx;
using Stardust.Starterkit.Proxy;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Stardust.Starterkit.Proxy
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {

                        c.Schemes(new[] { "https" });


                        c.SingleApiVersion("v1", "Stardust.Starterkit.Proxy");

                        c.ApiKey("key").Description("API token key").Name("key").In("header");
                        c.ApiKey("token").Description("token")
                        .Name("Authorization")
                        .In("header");
                        
                        c.IgnoreObsoleteActions();
                        c.IgnoreObsoleteProperties();
                        c.MapType<TransferMode>(() => new Schema { type = "integer", format = "int32" });

                        c.OperationFilter<TokeAuthFilter>();

                    })
                .EnableSwaggerUi(c =>
                    {

                    });
        }
    }

    public class TokeAuthFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.security == null) operation.security = new List<IDictionary<string, IEnumerable<string>>>();
            operation.security.Add(new Dictionary<string, IEnumerable<string>> { { "token", new List<string> {  } } });
            operation.security.Add(new Dictionary<string, IEnumerable<string>> { { "key", new List<string> {  } } });
        }
    }
}
