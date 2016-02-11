using Stardust.Core.Default.Implementations;
using Stardust.Core.Service.Web;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Serializers;

namespace StardustWebApplicationTemplate.App_Start
{
    public class WebBlueprint:Blueprint
    {
        /// <summary>
        /// Place your bindings here
        /// </summary>
        protected override void DoCustomBindings()
        {
            Configurator.Bind<IReplaceableSerializer>().To<JsonReplaceableSerializer>().SetSingletonScope();
            Configurator.Bind<IConfigurationReader>().To<StarterkitConfigurationReader>().SetSingletonScope();//update this to add caching and other features not supported in the standard
            Configurator.Bind<ISupportCodeGenerator>().To<SupportCodeGenerator>().SetSingletonScope();
            //override the default bindings here....
            base.DoCustomBindings();
        }

        /// <summary>
        /// Override this to change add custom Stardust.Core.Services bindings. 
        /// </summary>
        /// <remarks>
        /// If overridden the following needs to be bound:
        ///             <see cref="T:Stardust.Interstellar.ConfigurationReader.IConfigurationReader"/><see cref="T:Stardust.Interstellar.Utilities.IUrlFormater"/>
        ///             and <see cref="T:Stardust.Interstellar.IServiceTearDown"/>
        /// </remarks>
        protected override void SetDefaultBindings()
        {
            //override the default bindings here....
            base.SetDefaultBindings();
        }
    }
}