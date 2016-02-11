using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Nucleus.TypeResolver;
using IContainer = Autofac.IContainer;
using Xunit;

namespace LoadTest
{
    public class BenchmarkTest : IDisposable
    {
        private const string ServiceName = "newTeest";
        internal static int Iterations;
        internal static Type Type;
        internal static ConstructorInfo Constructor;
        private IKernelContext KernelScope;

        private KeyValuePair<TimeSpan, TimeSpan> Transient = new KeyValuePair<TimeSpan, TimeSpan>();
        private KeyValuePair<TimeSpan, TimeSpan> Singleton = new KeyValuePair<TimeSpan, TimeSpan>();
        private KeyValuePair<TimeSpan, TimeSpan> TransientWithInjection = new KeyValuePair<TimeSpan, TimeSpan>();

        public BenchmarkTest()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();
        }

        void IDisposable.Dispose()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        [Fact]
        public void TestExecution()
        {
            var container = PrepairTestRigg();
            BaseLineBenchmark();

            ObjectFactoryBenchmark();

            StardustIocBenchmark();

            AutoFacBenchmark(container);
            Assert.True(Transient.Key < Transient.Value);
            Assert.True(Singleton.Key < Singleton.Value);
            Assert.True(TransientWithInjection.Key < TransientWithInjection.Value);
        }

        private void AutoFacBenchmark(IContainer container)
        {
            using (container.BeginLifetimeScope())
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Continuing with AutoFac tests");
                Stopwatch timer;
                TimeSpan elapsed5;
                timer = Stopwatch.StartNew();
                for (int i = 0; i < Iterations; i++)
                {
                    var instance = container.Resolve<ITestClass>();
                }
                timer.Stop();
                elapsed5 = timer.Elapsed;
                Singleton = new KeyValuePair<TimeSpan, TimeSpan>(Singleton.Key, elapsed5);
                Console.WriteLine("{0} instances created with singleton resolves  in {1} ms", Iterations, elapsed5.TotalMilliseconds);


                timer = Stopwatch.StartNew();
                for (int i = 0; i < Iterations; i++)
                {
                    var instance = container.Resolve<ITestClass2>();
                }
                timer.Stop();
                elapsed5 = timer.Elapsed;
                Transient = new KeyValuePair<TimeSpan, TimeSpan>(Transient.Key, elapsed5);
                Console.WriteLine("{0} instances created with transient resolves  in {1} ms", Iterations, elapsed5.TotalMilliseconds);

                timer = Stopwatch.StartNew();
                for (int i = 0; i < Iterations; i++)
                {
                    var instance = container.ResolveNamed<ITestClass>(ServiceName);
                }
                timer.Stop();
                elapsed5 = timer.Elapsed;

                Console.WriteLine("{0} instances created with transient resolves by name {1} ms", Iterations,
                    elapsed5.TotalMilliseconds);

                timer = Stopwatch.StartNew();
                for (int i = 0; i < Iterations; i++)
                {
                    var instance = container.Resolve<ITestClass3>();
                    if (instance.TClass == null) Console.WriteLine("Injection failed");
                }
                timer.Stop();
                elapsed5 = timer.Elapsed;
                TransientWithInjection = new KeyValuePair<TimeSpan, TimeSpan>(TransientWithInjection.Key, elapsed5);
                Console.WriteLine("{0} instances created with transient resolves with injection in {1} ms", Iterations, elapsed5.TotalMilliseconds);
            }
        }

        private void StardustIocBenchmark()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Continuing with Resolver tests");
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                var instance = Resolver.Activate<ITestClass>();
            }
            timer.Stop();
            var elapsed4 = timer.Elapsed;
            Console.WriteLine("{0} instances created with singleton resolves in {1} ms", Iterations, elapsed4.TotalMilliseconds);
            Singleton = new KeyValuePair<TimeSpan, TimeSpan>(elapsed4, Singleton.Value);
            timer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                var instance = Resolver.Activate<ITestClass2>();
            }
            timer.Stop();
            var elapsed5 = timer.Elapsed;
            Console.WriteLine("{0} instances created with transient resolves in {1} ms", Iterations, elapsed5.TotalMilliseconds);
            Transient = new KeyValuePair<TimeSpan, TimeSpan>(elapsed5, Transient.Value);
            timer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                var instance = Resolver.Activate<ITestClass>(ServiceName);
            }
            timer.Stop();
            elapsed5 = timer.Elapsed;
            Console.WriteLine("{0} instances created with transient resolves by name in {1} ms", Iterations, elapsed5.TotalMilliseconds);

            timer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                var instance = Resolver.Activate<ITestClass3>();
                if (instance.TClass == null) Console.WriteLine("Injection failed");
            }
            timer.Stop();
            elapsed5 = timer.Elapsed;
            TransientWithInjection = new KeyValuePair<TimeSpan, TimeSpan>(elapsed5, TransientWithInjection.Value);
            Console.WriteLine("{0} instances created with transient resolves with injection in {1} ms", Iterations, elapsed5.TotalMilliseconds);
        }

        private static void ObjectFactoryBenchmark()
        {
            Stopwatch timer;
            timer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var instance = ObjectFactory.CreateConstructor(Constructor)();
            }
            timer.Stop();
            var elapsed2 = timer.Elapsed;
            Console.WriteLine("{0} instances created with CreateConstructor in {1}ms", Iterations, elapsed2.TotalMilliseconds);

            timer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var instance = ObjectFactory.CreateConstructor(Type)();
            }
            timer.Stop();
            var elapsed3 = timer.Elapsed;
            Console.WriteLine("{0} instances created with CreateConstructor from type in {1}ms", Iterations,
                elapsed3.TotalMilliseconds);
        }

        private static void BaseLineBenchmark()
        {
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var instance = new TestClass();
            }
            timer.Stop();
            var elapsed = timer.Elapsed;
            Console.WriteLine("{0} instances created with 'new' in {1}ms", Iterations, elapsed.TotalMilliseconds);

            timer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var instance = new TestClass3(new TestClass2());
            }
            timer.Stop();
            elapsed = timer.Elapsed;
            Console.WriteLine("{0} instances created with 'new'('new') in {1}ms", Iterations, elapsed.TotalMilliseconds);

            timer = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var instance = Activator.CreateInstance(Type);
            }
            timer.Stop();
            elapsed = timer.Elapsed;
            Console.WriteLine("{0} instances created with Activator.CreateInstance in {1}ms", Iterations, elapsed.TotalMilliseconds);
        }

        private static IContainer PrepairTestRigg()
        {
            SetUpStardust();
            var container = SetUpAutoFac();
            Iterations = 1000000;
            Type = typeof(TestClass);
            Constructor = Type.GetConstructors().First();
            ObjectFactory.CreateConstructor(Constructor)();
            ObjectFactory.CreateConstructor(Type)();
            Console.WriteLine("Testrig created");
            return container;
        }

        private static void SetUpStardust()
        {
            Resolver.LoadModuleConfiguration<Blueprint>();
            Resolver.GetConfigurator().Bind<ITestClass>().To<TestClass>().SetSingletonScope();
            Resolver.GetConfigurator().Bind<ITestClass>().ToConstructor(() => new TestClass(), ServiceName).SetSingletonScope();
            Resolver.GetConfigurator().Bind<ITestClass2>().To<TestClass2>().SetTransientScope();
            Resolver.GetConfigurator().Bind<ITestClass3>().To<TestClass3>().SetTransientScope();
            Resolver.Activate<ITestClass>();
            Resolver.Activate<ITestClass2>();
            Resolver.Activate<ITestClass3>();
            ContainerFactory.Current.KillAllInstances();
        }

        private static IContainer SetUpAutoFac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass>().As<ITestClass>().SingleInstance();
            builder.Register((context) => new TestClass()).As<ITestClass>().Named<ITestClass>(ServiceName).InstancePerDependency();
            builder.RegisterType<TestClass2>().As<ITestClass2>().InstancePerDependency();
            builder.RegisterType<TestClass3>().As<ITestClass3>().InstancePerDependency();
            var container = builder.Build();
            using (container.BeginLifetimeScope())
            {
                container.Resolve<ITestClass>();
                container.Resolve<ITestClass2>();
                container.Resolve<ITestClass3>();
                container.ResolveNamed<ITestClass>(ServiceName);
            }
            return container;
        }
    }
}