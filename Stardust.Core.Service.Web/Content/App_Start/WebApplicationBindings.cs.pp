using Stardust.Core.CrossCutting;
using Stardust.Core.Services;
using $rootnamespace$;

namespace $rootnamespace$.App_Start
{
    public class WebApplicationBindings : ServicesBindingConfiguration<LoggingDefaultImplementation>
    {
        protected override void DoCustomBindings()
        {
            //Add application bindings here
            //Resolver.Bind<IMyInterface>().To<MyImplementation>().SetTransientScope();
        }
    }
}