using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Clusters;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles.TableParser;
using Stardust.Particles.TabularMapper;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class TabularMapperTests
    {
        private IKernelContext KernelScope;

        [TestInitialize]
        public void Initialize()
        {
            KernelScope = Resolver.BeginKernelScope();
            Resolver.LoadModuleConfiguration<Blueprint>();

        }

        [TestCleanup]
        public void Cleanup()
        {
            Resolver.EndKernelScope(KernelScope, true);
        }

        private const string TestFilePath = @"C:\Users\jonsyr\Source\Workspaces\NGF-TERS\Main\Stardust\Stardust.Core.CrossCuttingTest\TestFiles\TabularMappingTestFile.txt";
        [TestMethod()]
        [TestCategory("Tabular Mapper")]
        public void ConvertSimpleStringType()
        {
            var data = Parsers.Delimitered.GetParser().SetHeaderRow(false)
                .Parse(TestFilePath)
                .ConvertTo<StringClass>(new MappingDefinition
                {
                    Fields =
                        new[]
                        {
                            new FieldMapping
                            {
                                SourceColumnNumber = 0,
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header1"
                            },
                            new FieldMapping
                            {
                                SourceColumnNumber = 1,
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header2"
                            },
                            new FieldMapping
                            {
                                SourceColumnNumber = 2,
                                MemberType = MemberTypes.Method,
                                TargetMemberName = "header3"
                            },
                            new FieldMapping
                            {
                                SourceColumnNumber = 3,
                                MemberType = MemberTypes.Field,
                                TargetMemberName = "header4"
                            },
                        }
                });
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        [TestCategory("Tabular Mapper")]
        public void ConvertSimpleTypedType()
        {
            var source = Parsers.Delimitered.GetParser().SetHeaderRow(true).Parse(TestFilePath);
            var data = source.ConvertTo<TypedClass>(new MappingDefinition
                {
                    Fields =
                        new[]
                        {
                            new FieldMapping
                            {
                                SourceColumnName = "header1",
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header1"
                            },
                            new FieldMapping
                            {
                                SourceColumnName = "header2",
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header2"
                            },
                            new FieldMapping
                            {
                                SourceColumnName = "header3",
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header3"
                            },
                            new FieldMapping
                            {
                                SourceColumnName = "header4",
                                MemberType = MemberTypes.Property,
                                TargetMemberName = "header4"
                            },
                        }
                });
            Assert.AreEqual(data.First().header1, ((dynamic)source.First()).header1);
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        [TestCategory("Tabular Mapper")]
        public void ConvertSimpleTypedTypeWithAutoMap()
        {
            var doc = Parsers.Delimitered.GetParser().SetHeaderRow(true)
                .Parse(TestFilePath);
            var data = doc.ConvertTo<TypedClass>(MappingDefinition.CreateMappingDefinition().AddMappingFromDocumentWithHeaders(doc));
            Assert.IsTrue(data.Any());
        }
    }

    
}