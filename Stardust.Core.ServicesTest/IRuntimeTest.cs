//using System;
//using System.Collections.Concurrent;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Pragma.Core.CrossCutting;
//using Pragma.Core.FactoryHelpers;
//using Pragma.Core.Services;
//using Pragma.Core.Services.Tasks;
//using Pragma.Core.ServicesTest.Mocks;
//using Pragma.Core.ServicesTest.ServiceReference1;

//namespace Pragma.Core.ServicesTest
//{
//    [TestClass()]
//    public class IRuntimeTest
//    {
//        private TestContext testContextInstance;

//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        [ClassInitialize]
//        public static void MyClassInitialize(TestContext testContext)
//        {
//            Resolver.Bind<ITestTask>().To<TestTask>();
//            GlobalAspectConfigurator.LoadGlobalPackage(new AspectPackageConfiguration());
//        }

//        internal virtual IRuntime CreateIRuntime()
//        {
//            return RuntimeFactory.CreateRuntime(Scope.PerRequest);
//        }

//        [TestMethod]
//        [Ignore]
//        public void BeginExcecuteTest()
//        {
//            IsHandled = false;
//            var target = CreateIRuntime();
//            target.BeginExecuteTask<RuntimeTaskTest, Message, Message>(new Message { MessageId = new Guid().ToString() },null);
//            Assert.IsTrue(IsHandled);
//        }

//        private bool IsHandled;
//        public void HandleAsyncMessage(Message message)
//        {
//            Assert.IsTrue(message.IsInstance());
//            IsHandled = true;
//        }

//        [TestMethod()]
//        public void CloseProxyTest()
//        {
//            var target = CreateIRuntime();
//            var serviceContainer = target.CreateServiceProxy<Mocks.IContacts>();
//            serviceContainer.Dispose();
//        }

//        [TestMethod()]
//        public void CreateRuntimeTaskTest()
//        {
//            var target = CreateIRuntime();
//            var task = target.CreateRuntimeTask<ITestTask>();
//            Assert.IsTrue(task.IsInstance());
//        }

//        [TestMethod()]
//        public void CreateRuntimeTaskTest1()
//        {
//            var target = CreateIRuntime();
//            var task = target.CreateRuntimeTask<ITestTask>(Scope.Singleton);
//            Assert.IsInstanceOfType(task.CreateNewObject(), typeof(object));
//            Assert.IsTrue(task.IsInstance());
//        }

//        [TestMethod()]
//        public void ExcecuteTest()
//        {
//            var target = CreateIRuntime();
//            var message = new Message { MessageId = new Guid().ToString() };
//            var actual = target.Execute<RuntimeTaskTest, Message, Message>(message,null);
//            Assert.IsTrue(actual.GetExecutionTimeAsTimeSpan() >= new TimeSpan(0, 0, 0, 0, 19));
//            Assert.AreEqual(message.MessageId, actual.MessageId);
//        }

//        /// <summary>
//        ///A test for InitializeWithMessageContext
//        ///</summary>
//        [TestMethod()]
//        public void InitializeWithMessageContextTest()
//        {
//            var target = CreateIRuntime();
//            var message = new Message { Environment = "Dev" };
//            var actual = target.InitializeWithMessageContext(message);
//            Assert.AreSame(actual, target);
//        }


//        /// <summary>
//        ///A test for Context
//        ///</summary>
//        [TestMethod()]
//        public void ContextTest()
//        {
//            var target = CreateIRuntime(); // TODO: Initialize to an appropriate value
//            var actual = target.Context;
//            Assert.IsInstanceOfType(actual, typeof(IRuntimeContext));
//        }


//        [TestMethod]
//        public void CreateServiceTest()
//        {
//            var target = CreateIRuntime();
//            var expected = target.Context;
//            var actual = target.CreateNewService<ContactsClient>("ContactsService");
//            Assert.AreSame(expected, actual.GetRuntimeContext().Context);
//        }

//        [TestMethod]
//        public void CreateNamedServiceTest()
//        {
//            var target = CreateIRuntime();
//            var expected = target.Context;
//            using (var actual = target.CreateNewService<ContactsClient>("ContactsService"))
//            {
//                Assert.AreSame(expected, actual.GetRuntimeContext().Context);
//            }
//        }

//        [TestMethod]
//        public void Test()
//        {
//            var target = CreateIRuntime();
//            var myType = target.CreateRuntimeTask<IMyType<string>>(Scope.PerRequest, typeof(string));
//            Assert.IsNotNull(myType);
//        }

//        [TestMethod]
//        public void CreateNonProxyInstance()
//        {
//            Resolver.Bind<IMyType<long>>().To<MyType<long>>("type1");
//            Resolver.Bind<IMyType<long>>().To<MyType2<long>>("type2");
//            var myInstance = ModuleCreator.CreateInstance<IMyType<long>>(new ObjectInitializer { Name = "type1" }, () => typeof(long));
//            Assert.IsInstanceOfType(myInstance, typeof(MyType<long>));
//            myInstance = ModuleCreator.CreateInstance<IMyType<long>>(new ObjectInitializer { Name = "type2" }, () => typeof(long));
//            Assert.IsInstanceOfType(myInstance, typeof(MyType2<long>));
//        }

//    }

//    public interface IMyType<T> : IRuntimeTask
//    {
//    }

//    class MyType2<T> : IMyType<T>
//    {
//        public IRuntimeTask Initialize(Services.Messaging.IRequestBase message)
//        {
//            throw new NotImplementedException();
//        }

//        public IRuntimeTask Initialize(string environment)
//        {
//            throw new NotImplementedException();
//        }

//        public IRuntimeTask Initialize(string requestId, string configSetPath)
//        {
//            throw new NotImplementedException();
//        }

//        public IRuntimeTask SetExternalState(ref IRuntime runtime)
//        {
//            return this;
//        }

//        public bool SupportsAsync
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public ConcurrentStack<string> CallStack
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public void SetCallStack(ConcurrentStack<string> callStack)
//        {
//            throw new NotImplementedException();
//        }


//        public IRuntimeTask SetInvokerStateStorage(ref StateStorageItem stateItem)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    class MyType<T> : IMyType<T>
//    {
//        public IRuntimeTask Initialize(Services.Messaging.IRequestBase message)
//        {
//            return this;
//        }

//        public IRuntimeTask Initialize(string environment)
//        {
//            return this;
//        }

//        public IRuntimeTask Initialize(string requestId, string configSetPath)
//        {
//            return this;
//        }

//        public IRuntimeTask SetExternalState(ref IRuntime runtime)
//        {
//            return this;
//        }

//        public bool SupportsAsync
//        {
//            get { return false; }
//        }

//        public ConcurrentStack<string> CallStack { get; private set; }

//        public void SetCallStack(ConcurrentStack<string> callStack)
//        {
//            CallStack = callStack;
//        }


//        public IRuntimeTask SetInvokerStateStorage(ref StateStorageItem stateItem)
//        {
//            return this;
//        }
//    }

//    public class AspectPackageConfiguration : BasicAspectConfigurationPackage
//    {
//        protected override void DoBind(AspectConfigurator toConfigurator)
//        {
//            toConfigurator.BindToNamespace<ExceptionLoggingAspect>("Simple");
//            toConfigurator.Bind<MessageProcessingTimerAspect, MessageProcessingTimerAttribute>(1);
//            toConfigurator.Bind<RuntimeInitializerAspect, RuntimeInitializerAttribute>(0);
//            FactoryHelpers.Container.BindProviders();
//        }
//    }
//}
