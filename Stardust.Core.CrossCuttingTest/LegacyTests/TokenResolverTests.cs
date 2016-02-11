using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Stardust.Core.Default.Implementations;
using Stardust.Core.Security;
using Stardust.Interstellar;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Interstellar.Messaging;
using Stardust.Interstellar.Serializers;
using Stardust.Interstellar.Utilities;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class TokenResolverTests
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<TestModuleBindingConfiguration>();

        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        [TestMethod]
        [TestCategory("ADFS thumbprint resolver")]
        public void GetThumbprintTest()
        {
            var thbpr = ThumbprintResolver.ResolveThumbprint("123","https://stsbridge.cloudapp.net/federationmetadata/2007-06/federationmetadata.xml");
            Assert.AreNotEqual("123",thbpr);
            Assert.AreEqual("E61E395A7C1AF3730E8AFBD256D913FD757E671E",thbpr);
        }

        [TestMethod]
        public void ServiceContainer_profileTest()
        {
            ServiceRegistrationServer.RegisterServiceEndpoint<IAsyncApplicationService>("secure");
            {
                for (var i = 0; i < 10; i++)
                {
                    using (ContainerFactory.Current.ExtendScope(Scope.Context))
                    {
                        var timer = Stopwatch.StartNew();
                        using (var container = RuntimeFactory.CreateRuntime().CreateServiceProxy<IAsyncApplicationService>())
                        {
                            container.Initialize(true);
                            var client = container.GetClient();
                        }
                        timer.Stop();
                        Console.WriteLine("Created in {0}ms", timer.ElapsedMilliseconds);
                    }

                }
            }
        }
    }

    public abstract class tSampleModuleConfigurationBase : Blueprint<LoggingDefaultImplementation>
    {
        protected override void DoCustomBindings()
        {
            Resolver.Bind<IReplaceableSerializer>().To<ProtobufReplaceableSerializer>().SetSingletonScope();
        }

        protected override void SetDefaultBindings()
        {
            //Bind<IConfigurationReader, AppFabricConfigReader>().SetSingletonScope().AllowOverride = false;
            Bind<IConfigurationReader, StarterkitConfigurationReader>().SetSingletonScope();
            Bind<IUrlFormater, UrlFormater>().SetSingletonScope().AllowOverride = false;
            SetServiceTearDown<SampleServiceTearDown>();
        }
    }
    public class TestModuleBindingConfiguration : tSampleModuleConfigurationBase
    {
        protected override void DoCustomBindings()
        {
            base.DoCustomBindings();
        }
    }

    public class SampleServiceTearDown : IServiceTearDown
    {
        private const string LogPath = @"C:\temp\logging\{0}.json";

        void IServiceTearDown.TearDown(IRuntime runtime, Exception exception)
        {
            LoggExecution(runtime);
            ThreadPool.QueueUserWorkItem(task => Logging.Exception(exception, string.Format("Exception thrown in runtime instance: {0}", runtime.InstanceId)));
        }

        void IServiceTearDown.TearDown(IRuntime runtime, string payload)
        {
            LoggExecution(runtime);
        }

        T IServiceTearDown.TearDown<T>(IRuntime runtime, T message)
        {
            LoggExecution(runtime);
            message = AddMetricsToMessage(runtime, message);
            CalculateExecutionTime(runtime, message);
            ThreadPool.QueueUserWorkItem(task => Logging.DebugMessage(string.Format("Runtime instance {0} completed in {1}ms", runtime.InstanceId, runtime.CallStack.ExecutionTime)));
            return message;
        }


        private static T AddMetricsToMessage<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if(message.ResponseHeader.IsNull())message.ResponseHeader=new ResponseHeader();
            if (message.ResponseHeader.MessageId.IsNullOrWhiteSpace())
                message.ResponseHeader.MessageId = Guid.NewGuid().ToString();
            if (message.ResponseHeader.TimeStamp == null)
                message.ResponseHeader.TimeStamp = DateTime.UtcNow;
            message.ResponseHeader.RuntimeInstance = runtime.InstanceId.ToString();
            message.ResponseHeader.OriginalRuntimeInstance = runtime.RequestContext.IsInstance() ? runtime.RequestContext.RequestHeader.RuntimeInstance : "";
            if (message.ResponseHeader.ReferingMessageId.IsNullOrWhiteSpace())
                message.ResponseHeader.ReferingMessageId = runtime.RequestContext.IsInstance() ? runtime.RequestContext.RequestHeader.MessageId : "";
            AppendCallStack(runtime, message);
            return message;
        }

        private static void AppendCallStack<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if (runtime.Context.GetEnvironmentConfiguration().GetConfigParameter("disableCallStackPropagation") != "true")
                message.ResponseHeader.CallStack = runtime.CallStack;
        }

        internal static void CalculateExecutionTime<T>(IRuntime runtime, T message) where T : IResponseBase
        {
            if (runtime.CallStack.EndTimeStamp.HasValue)
                message.ResponseHeader.ExecutionTime = (long)(runtime.CallStack.EndTimeStamp.Value - runtime.CallStack.TimeStamp).TotalMilliseconds;
        }

        private void LoggExecution(IRuntime runtime)
        {
            var filename = runtime.InstanceId.ToString();
            var request = runtime.GetServiceRequest<IRequestBase>();
            if (request.IsInstance())
            {
                filename = string.Format("{0}_{1}", request.RequestHeader.RuntimeInstance, runtime.InstanceId);
            }
            var callstack = runtime.CallStack;
            ThreadPool.QueueUserWorkItem(task => LogCallstack(filename, callstack));
        }

        private async static void LogCallstack(string filename, CallStackItem callstack)
        {
            var file = string.Format(LogPath, filename);
            using (var stream = File.OpenWrite(file))
            {
                var buffer = JsonConvert.SerializeObject(callstack).GetByteArray();
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
            }
        }
    }
    [ServiceContract(Namespace = "http://stardust.com/sample/ApplicationService")]
    [ServiceName("AsyncApplicationService")]
    public interface IAsyncApplicationService
    {
        [OperationContract()]
        void ChangePasswordTaskAsync(ChangePasswordMessage request);
    }

    public class ChangePasswordMessage:RequestBase
    {
    }
}
