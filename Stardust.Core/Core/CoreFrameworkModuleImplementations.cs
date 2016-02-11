using System;
using Stardust.Clusters;
using Stardust.Core.DynamicCompiler;
using Stardust.Core.Security;
using Stardust.Interstellar.Endpoints;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Particles.Converters;
using Stardust.Particles.EncodingCheckers;
using Stardust.Particles.FileTransfer;
using Stardust.Particles.TableParser;
using Stardust.Particles.TabularMapper;
using Stardust.Wormhole.Converters;
using Stardust.Wormhole.MapBuilder;
using Stardust.Interstellar.Trace;

namespace Stardust.Core
{
    /// <summary>
    /// Loads the framework bindings using the default logger.
    /// </summary>
    internal class CoreFrameworkBlueprint : CoreFrameworkBlueprint<LoggingDefaultImplementation>
    {
        
    }

    /// <summary>
    /// Loads the framework bindings using a provided logging implementation
    /// logger.
    /// </summary>
    /// <typeparam name="T">
    /// An implementation of the ILogging <see langword="interface"/>
    /// </typeparam>
    internal class CoreFrameworkBlueprint<T> : IBlueprint
    {
        internal protected virtual IConfigurator Resolver {get; private set; }

        public void Bind(IConfigurator resolver)
        {
            Resolver = resolver;
            BindFactories(Resolver);
            FrameworkBindings();
            Resolver.Bind<ITracer>().To<Tracer>().SetTransientScope().DisableOverride();
        }

        /// <summary>
        /// The <see cref="Type" /> of the <see cref="ILogging"/> implementation.
        /// </summary>
        public virtual Type LoggingType
        {
            get { return typeof (T); }
        }

        /// <summary>
        /// This should only be called by Stardust. This is where we apply all
        /// bindings that are required for the framework to work and have a
        /// consistent behavior.
        /// </summary>
        protected internal virtual void FrameworkBindings()
        {}

        private static void BindFactories(IConfigurator Resolver)
        {
            BindEncodingFactory(Resolver);
            BindParserFactory(Resolver);
            BindTransferFactory(Resolver);
            BindBinaryConverterFactory(Resolver);
            BindTypeConverters(Resolver);
            BindMiscStuff(Resolver);
            BindWcfBindingCreators();
        }

        static void BindWcfBindingCreators()
        {
            BindingFactory.BindNew<WsBindingCreator>("ws");
            BindingFactory.BindNew<WsBindingCreator>("soap");
            BindingFactory.BindNew<BasicBindingCreator>("basic");
            BindingFactory.BindNew<TcpBindingCreator>("tcp");
            BindingFactory.BindNew<PipeBindingCreator>("pipe");
            BindingFactory.BindNew<MsmqBindingCreator>("msmq");
            BindingFactory.BindNew<RestBindingCreator>("rest");
            BindingFactory.BindNew<SecureRestBindingCreator>("securerest");
            BindingFactory.BindNew<SecureBindingCreator>("secure");
            BindingFactory.BindNew<StsBindingCreator>("sts");
            BindingFactory.BindNew<WindowsAuthenticatedWsBindingCreator>("auth");
            BindingFactory.BindNew<CustomBindingCreator>("custom");
        }

        private static void BindMiscStuff(IConfigurator Resolver)
        {
            Resolver.Bind<ITabularMapper>().To<TabularMapper>().SetSingletonScope().AllowOverride = false;
            Resolver.Bind<ICodeInjector>().To<CSharpInjector>(LanguageType.CSharp);
            Resolver.Bind<ICodeInjector>().To<VbCodeInjector>(LanguageType.VisualBasic);
            Resolver.Bind<IThumbprintCache>().To<InProcThumbprintCache>().SetTransientScope();
        }

        private static void BindTypeConverters(IConfigurator Resolver)
        {
            Resolver.Bind<ITypeConverter>().To<SimpleTypeConverter>(BasicTypes.ComplexType).SetTransientScope().AllowOverride = false;
            Resolver.Bind<ITypeConverter>().To<ConvertableTypeConverter>(BasicTypes.Convertable).SetTransientScope().AllowOverride = false;
            Resolver.Bind<ITypeConverter>().To<DictionaryTypeConverter>(BasicTypes.Dictionary).SetTransientScope().AllowOverride = false;
            Resolver.Bind<ITypeConverter>().To<EnumerableTypeConverter>(BasicTypes.List).SetTransientScope().AllowOverride = false;
            Resolver.Bind<ITypeConverter>().To<FlatenComplexType>(BasicTypes.FlatenComplexType).SetTransientScope().AllowOverride = false;
            Resolver.Bind<ITypeConverter>().To<ExpanderTypeConverter>(BasicTypes.Expander).SetTransientScope().AllowOverride = false;
        }

        private static void BindBinaryConverterFactory(IConfigurator Resolver)
        {
            Resolver.Bind<IBinaryConverter>().To<HexConverter>(ConverterTypes.Hex).SetSingletonScope().DisableOverride();
            Resolver.Bind<IBinaryConverter>().To<BinaryUtf8Converter>(ConverterTypes.BinaryUtf8).SetSingletonScope().DisableOverride();
            Resolver.Bind<IBinaryConverter>().To<BinaryUnicodeConverter>(ConverterTypes.BinaryUnicode).SetSingletonScope().DisableOverride();
        }

        private static void BindTransferFactory(IConfigurator Resolver)
        {
            Resolver.Bind<IFileTransfer>().To<FileTransfer>(TransferMethods.File).SetTransientScope().DisableOverride();
            Resolver.Bind<IFileTransfer>().To<HttpFileTrasfer>(TransferMethods.Http).SetTransientScope().DisableOverride();
            Resolver.Bind<IFileTransfer>().To<FtpTrasfer>(TransferMethods.Ftp).SetTransientScope().DisableOverride();
            Resolver.Bind<IFileTransfer>().To<FileTransfer>().SetTransientScope().DisableOverride();
        }

        private static void BindParserFactory(IConfigurator Resolver)
        {
            Resolver.Bind<ITableParser>().To<XmlTableParser>(Parsers.SimpleXmlParser).SetTransientScope().DisableOverride();
            Resolver.Bind<ITableParser>().To<CsvTableParser>(Parsers.Delimitered).SetTransientScope().DisableOverride(); ;
            Resolver.Bind<ITableParser>().To<XlsTableParser>(Parsers.OldExcel).SetTransientScope().DisableOverride(); ;
            Resolver.Bind<ITableParser>().To<XlsxTableParser>(Parsers.Excel).SetTransientScope().DisableOverride(); ;
        }

        private static void BindEncodingFactory(IConfigurator Resolver)
        {
            Resolver.Bind<IEncodingChecker>().To<UnicodeBigendianChecker>("1").SetSingletonScope().DisableOverride();
            Resolver.Bind<IEncodingChecker>().To<UnicodeChecker>("2").SetSingletonScope().DisableOverride();
            Resolver.Bind<IEncodingChecker>().To<Utf32Checker>("3").SetSingletonScope().DisableOverride();
            Resolver.Bind<IEncodingChecker>().To<Utf8Checker>("4").SetSingletonScope().DisableOverride();
            Resolver.Bind<IEncodingChecker>().To<Utf7Checker>("5").SetSingletonScope().DisableOverride();
        }
    }
}