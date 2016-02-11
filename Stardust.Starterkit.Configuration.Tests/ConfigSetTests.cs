using System;
using System.Linq;
using System.Reflection;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Starterkit.Configuration.Business;
using Stardust.Starterkit.Configuration.Repository;
using Xunit;

namespace Stardust.Starterkit.Configuration.Tests
{
    [Trait("Config", "ConfigSet")]
    public class ConfigSetTests:IDisposable
    {
        private const string SystemName = "Test";
        private const string configSetName1 = "ToBeDeleted";
        private const string configSetName2 = "firstChild";
        private IKernelContext KernelScope;
        

        public ConfigSetTests()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<TestConfigServiceBindings>();
        }

        void IDisposable.Dispose()
        {
            Resolver.EndKernelScope(KernelScope,true);
        }
        

        [Fact]
        public void CreateConfigSetWithServiceTest()
        {
            CleanUp();
            using (var csController = ConfigReaderFactory.GetConfigSetTask())
            {
                csController.CreateConfigSet(configSetName1, SystemName, null);
                var cs = csController.GetConfigSet(configSetName1, SystemName);
                var environment = csController.CreatEnvironment(cs, "Dev");
                var service = csController.CreateService(cs, "ServiceName");
                var enpoint = csController.CreateEndpoint(service, "EndpointName");
                Assert.Equal(configSetName1, cs.Name);
                Assert.Equal(SystemName, cs.System);
                Assert.Equal("ServiceName", service.Name);
                Assert.Equal(cs.Id, service.ConfigSetNameId);
                Assert.True(service.Id.Contains(cs.Id));
                Assert.Equal(27, enpoint.Parameters.Count());
                Assert.Equal(1,cs.Environments.Count);
                Assert.Equal("Dev",environment.Name);
                Assert.Equal(20,environment.SubstitutionParameters.Count);
            }
        }

        [Fact]
        public void CloneConfigSetWithServiceTest()
        {
            CleanUp();
            using (var csController = ConfigReaderFactory.GetConfigSetTask())
            {
                csController.CreateConfigSet(configSetName1, SystemName, null);
                var cs = csController.GetConfigSet(configSetName1, SystemName);
                var environment = csController.CreatEnvironment(cs, "Dev");
                var service = csController.CreateService(cs, "ServiceName");
                var enpoint = csController.CreateEndpoint(service, "EndpointName");
                
                Assert.Equal(configSetName1, cs.Name);
                Assert.Equal(SystemName, cs.System);
                Assert.Equal("ServiceName", service.Name);
                Assert.Equal(cs.Id, service.ConfigSetNameId);
                Assert.True(service.Id.Contains(cs.Id));
                Assert.Equal(27,enpoint.Parameters.Count());
                Assert.Equal(20, environment.SubstitutionParameters.Count);
                cs = csController.GetConfigSet(configSetName1, SystemName);


                csController.CreateConfigSet(configSetName2, SystemName, cs);
                var child = csController.GetConfigSet(configSetName2, SystemName);
                Assert.Equal(configSetName2, child.Name);
                Assert.Equal(SystemName, child.System);
                Assert.Equal(1,child.Services.Count);
                Assert.Equal(27, child.Services.First().Endpoints.First().Parameters.Count);
                Assert.Equal(20, child.Environments.First().SubstitutionParameters.Count);
                //return;
            }
        }

        [Fact]
        public void CreateChildSet()
        {
            CleanUp();
            using (var csController = ConfigReaderFactory.GetConfigSetTask())
            {
                csController.CreateConfigSet(configSetName1, SystemName, null);
                var cs = csController.GetConfigSet(configSetName1, SystemName);
                csController.CreateConfigSet(configSetName2, SystemName, cs);
                var child = csController.GetConfigSet(configSetName2, SystemName);
                Assert.Equal(configSetName1, cs.Name);
                Assert.Equal(SystemName, cs.System);
                Assert.Equal(configSetName2, child.Name);
                Assert.Equal(SystemName, child.System);
            }
        }

        [Fact]
        public void CreateDuplicateSet()
        {
            CleanUp();
            using (var csController = ConfigReaderFactory.GetConfigSetTask())
            {
                csController.CreateConfigSet(configSetName1, SystemName, null);
                var cs = csController.GetConfigSet(configSetName1, SystemName);

                Assert.Throws<AmbiguousMatchException>(
                    () => csController.CreateConfigSet(configSetName1, SystemName, null));
            }
        }

        private void CleanUp()
        {
            ContainerFactory.Current.KillAllInstances();
            using (var context = new RepositoryFactory().GetRepository())
            {
                try
                {
                    var sp = from i in context.SubstitutionParameters select i;
                    foreach (var substitutionParameter in sp)
                    {
                        context.DeleteObject(substitutionParameter);
                    }
                    context.SaveChanges();
                    var envp = from i in context.EnvironmentParameters select i;
                    foreach (var substitutionParameter in envp)
                    {
                        context.DeleteObject(substitutionParameter);
                    }
                    context.SaveChanges();
                    var envs = from env in context.Environments select env;
                    foreach (var environment in envs)
                    {
                        context.DeleteObject(environment);
                    }
                    context.SaveChanges();
                    var ep = from cs in context.EndpointParameters select cs;
                    foreach (var i in ep.ToList())
                    {
                        context.DeleteObject(i);
                    }
                    context.SaveChanges();
                    var e = from cs in context.Endpoints select cs;
                    foreach (var i in e.ToList())
                    {
                        context.DeleteObject(i);
                    }
                    context.SaveChanges();
                    var s = from cs in context.ServiceDescriptions select cs;
                    foreach (var i in s.ToList())
                    {
                        context.DeleteObject(i);
                    }
                    context.SaveChanges();
                    var css = from cs in context.ConfigSets select cs;
                    foreach (var i in css.ToList())
                    {
                        context.DeleteObject(i);
                    }
                    context.SaveChanges();
                    if (context.ConfigSets.Count() != 0) throw new Exception("Not able to clear database");
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            ContainerFactory.Current.KillAllInstances();
        }

        
    }

    public class TestConfigServiceBindings : Blueprint
    {
        protected override void DoCustomBindings()
        {
            Resolver.Bind<IConfigSetTask>().To<ConfigSetTask>().SetRequestResponseScope().AllowOverride = false;
            Resolver.Bind<IRepositoryFactory>().To<RepositoryFactory>().SetSingletonScope().AllowOverride = false;
            Resolver.Bind<IUserFacade>().To<UserFacade>().SetRequestResponseScope().AllowOverride = false;
        }
    }
}
