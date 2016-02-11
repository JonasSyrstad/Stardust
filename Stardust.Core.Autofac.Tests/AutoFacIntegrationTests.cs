using System;
using Stardust.Core.AutoFac;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;
using Stardust.Particles.EncodingCheckers;
using Xunit;

namespace Stardust.Core.Autofac.Tests
{
    public class AutoFacIntegrationTests : IDisposable
    {
        private IKernelContext KernelScope;

        public AutoFacIntegrationTests()
        {
            try
            {
                Resolver.LoadIocContainer(new AutoFacFactory());
            }
            catch (Exception)
            {
            }
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<AutoFacTestBindings>();
        }

        void IDisposable.Dispose()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        [Fact]
        public void SimpeleTest()
        {
            var encoder = Resolver.Activate<IEncodingChecker>("1");
            Assert.IsType<UnicodeBigendianChecker>(encoder);
        }

        //[Fact]
        //public void RuntimeFactoryTest()
        //{
        //    IRuntime runtime;

        //    var runtime1 = RuntimeFactory.CreateRuntime();
        //    var runtime2 = RuntimeFactory.CreateRuntime();
        //    Assert.NotNull(runtime1);
        //    Assert.NotNull(runtime2);
        //    Assert.Equal(runtime1, runtime2);
        //}

        [Fact]
        public void RuntimeFactoryInExtendedScope()
        {
            var runtime1 = RuntimeFactory.CreateRuntime(Scope.PerRequest);
            runtime1.InitializeWithConfigSetName("test");
            var runtime2 = RuntimeFactory.CreateRuntime(Scope.PerRequest);
            Assert.NotNull(runtime1);
            Assert.NotNull(runtime2);
            Assert.NotEqual(runtime1, runtime2);
        }

        //[Fact]
        public void RuntimeFactoryScopedTest()
        {

            IRuntime runtime1;
            using (Resolver.BeginExtendedScope(Scope.Context))
            {
                runtime1 = RuntimeFactory.CreateRuntime();
                runtime1.InitializeWithConfigSetName("test");
            }
            IRuntime runtime2;
            using (Resolver.BeginExtendedScope(Scope.Context))
            {
                runtime2 = RuntimeFactory.CreateRuntime();
            }
            Assert.NotNull(runtime1);
            Assert.NotNull(runtime2);
            Assert.NotEqual(runtime1, runtime2);
        }
    }
}