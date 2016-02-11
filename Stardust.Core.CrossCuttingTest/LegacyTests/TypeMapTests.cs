using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Core.CrossCuttingTest.LegacyTests.Mock;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Nucleus.TypeResolver;
using Stardust.Wormhole;
using Stardust.Wormhole.Converters;
using Stardust.Wormhole.MapBuilder;

namespace Stardust.Core.CrossCuttingTest.LegacyTests
{
    [TestClass]
    public class TypeMapTests
    {
        private static List<InnType> loadTestTarget;
        private static List<StringInnType> loadTestStringTarget;
        public TypeMapTests()
        {
            MapFactory.ResetAllMaps();
        }

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

        static TypeMapTests()
        {
            loadTestTarget = new List<InnType>
            {
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},

            };
            for (int i = 0; i < 100000; i++)
            {
                loadTestTarget.Add(new InnType { DateTimeValue = DateTime.Now, DesimalValue = i, StringValue = "test" });
            }
            loadTestStringTarget = new List<StringInnType>
            {new StringInnType{
                Value1 = "test1",
                Value2 = "Jonas",
                Value3 = "Syrstad",
                Email = "JonasSyrstad@outlook.com"},
                new StringInnType{
                Value1 = "test2",
                Value2 = "Jonas",
                Value3 = "Syrstad",
                Email = "JonasSyrstad@outlook.com"}
            };
            for (int i = 0; i < 100000; i++)
            {
                loadTestStringTarget.Add(new StringInnType
                {
                    Value1 = "test" + i,
                    Value2 = "Jonas",
                    Value3 = "Syrstad",
                    Email = "JonasSyrstad@outlook.com"
                });
            }
        }
        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void SingleObjectTest()
        {
            MapFactory.ResetAllMaps();
            var target = new InnType { DateTimeValue = DateTime.Now, DesimalValue = (decimal)100.212, StringValue = "test", MyEnum = Mock.MyEnum.something };
            var map = MapFactory.CreateEmptyMapRule<InnType, OutType>();
            map.AddMap<InnType, OutType>(new Dictionary<string, string>
            {
                {"DateTimeValue", "DateTimeValue1"},
                {"DesimalValue", "DesimalValue1"},
                {"StringValue", "StringValue1"}
            });
            map.GetRule<InnType, OutType>().Add(r => r.MyEnum, r => r.MyEnumName).Add(r => r.MyEnum, r => r.MyEnumVal);

            var result = target.Map().To<OutType>(map);
            Assert.IsNotNull(result);
            Assert.AreEqual(target.DateTimeValue.ToString("g"), result.DateTimeValue1);
            Assert.AreEqual(target.DesimalValue, result.DesimalValue1);
            Assert.AreEqual(target.StringValue, result.StringValue1);
            Assert.AreEqual("something", result.MyEnumName);
            Assert.AreEqual(2, result.MyEnumVal);
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void ListObjectTest()
        {
            MapFactory.ResetAllMaps();
            var target = new List<InnType>
            {
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"}
            };
            var map = MapFactory.CreateEmptyMapRule<InnType, OutType>();
            map.AddMap<InnType, OutType>(new Dictionary<string, string>
            {
                {"DateTimeValue", "DateTimeValue1"},
                {"DesimalValue", "DesimalValue1"},
                {"StringValue", "StringValue1"}
            });
            var result = target.AsEnumerable().Map().To<OutType>(map);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void ListObjectTestType2()
        {
            MapFactory.ResetAllMaps();
            var target = new List<InnType>
            {
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"}
            };
            var map = MapFactory.CreateEmptyMapRule<InnType, OutType2>();
            map.AddMap<InnType, OutType2>(new Dictionary<string, string>
            {
                {"DateTimeValue", "DateTimeValue"},
                {"DesimalValue", "DesimalValue1"},
                {"StringValue", "StringValue"}
            });
            var result = target.Map().To<OutType2>(map);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }
        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void ListObjectTestWithAutoMapping()
        {
            MapFactory.ResetAllMaps();
            var target = new List<InnType>
            {
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"},
                new InnType {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test",MyInts = new List<int>{1,2,3,4}}
            };

            var result = target.Map().To<OutType2>();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(target.First().DateTimeValue, result.First().DateTimeValue);
            Assert.AreNotEqual(target.First().DesimalValue, result.First().DesimalValue1);
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void Perf_ListObjectTestWithAutoMapping()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<InnType, OutType2>();
            //warm-up run 
            var wUpResult = loadTestTarget.Take(10).Map().To<OutType2>(map).ToList();

            var timer = Stopwatch.StartNew();
            var result = loadTestTarget.Map().To<OutType2>(map);
            Assert.AreEqual(100002, result.Count());
            timer.Stop();
            var time = timer.ElapsedTicks;

            Assert.IsNotNull(result);
            Assert.AreEqual((object)loadTestTarget.First().DateTimeValue, result.First().DateTimeValue);
            Assert.AreNotEqual((object)loadTestTarget.First().DesimalValue, result.First().DesimalValue1);
            if (new TimeSpan(time).TotalMilliseconds > 350)
                Assert.Fail("Mapping took {0}", new TimeSpan(time));
            #region comparison with IL
            //Just for comparison with pure IL and Linq
            //timer.Restart();
            //var ilTest = (from i in loadTestTarget
            //              select
            //                  new OutType2
            //                  {
            //                      DateTimeValue = i.DateTimeValue,
            //                      DesimalValue1 = i.DesimalValue,
            //                      StringValue = i.StringValue,
            //                      MyInts = i.MyInts == null ? null : (from s in i.MyInts select s.ToString()).ToArray()
            //                  }).ToList();
            //Assert.AreEqual(100002, ilTest.Count());
            //var referenceTime = timer.ElapsedTicks;
            //if (referenceTime * 250 < time)
            //    Assert.Fail("Mapping took {0} vs reference {1}", time, referenceTime); 
            #endregion
        }

        [TestMethod()]

        [TestCategory("Type Mapper")]
        public void Perf_ListStringsOnlyObjectTestWithAutoMapping()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<StringInnType, StringOutType1>();
            //warm-up run 
            var wUpResult = loadTestStringTarget.Take(10).Map().To<StringOutType1>(map).ToList();

            var timer = Stopwatch.StartNew();
            var result = loadTestStringTarget.Map().To<StringOutType1>(map);
            Assert.AreEqual(100002, result.Count());
            timer.Stop();
            var time = timer.ElapsedTicks;

            Assert.IsNotNull(result);
            Assert.AreEqual((object)loadTestStringTarget.First().Value1, result.First().Value1);
            Assert.AreEqual((object)loadTestStringTarget.First().Value3, result.First().Value3);
            if (new TimeSpan(time).TotalMilliseconds > 250)
                Assert.Fail("Mapping took {0}", new TimeSpan(time));
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void EnumerableListObjectTestWithAutoMapping()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<InnTypeEnum, OutTypeEnum>();
            var target = CreateTestTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<OutTypeEnum>();
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(timer.ElapsedMilliseconds < 20);
            Assert.IsNotNull(result);

            Assert.AreEqual((object)target.First().DateTimeValue, result.First().DateTimeValue);
            Assert.AreEqual(3, result.First().Child.Count());
            Assert.AreEqual(2, result.First().Child.First().Child.Count());
        }

        private static IEnumerable<InnTypeEnum> CreateTestTarget()
        {
            var target = new List<InnTypeEnum>
            {
                new InnTypeEnum
                {
                    DateTimeValue = DateTime.Now,
                    DesimalValue = (decimal) 100.212,
                    StringValue = "test",
                    Child =
                        new List<SubInType>
                        {
                            new SubInType
                            {
                                Name = "Test1",
                                Child = new List<SubInType>
                                {
                                    new SubInType {Name = "SubTest1"},
                                    new SubInType {Name = "SubTest2"}
                                }
                            },
                            new SubInType {Name = "Test2"},
                            new SubInType {Name = "Test3"}
                        }
                },
                new InnTypeEnum
                {
                    DateTimeValue = DateTime.Now,
                    DesimalValue = (decimal) 100.212,
                    StringValue = "test",
                    Child =
                        new List<SubInType>
                        {
                            new SubInType
                            {
                                Name = "Test1",
                                Child = new List<SubInType>
                                {
                                    new SubInType {Name = "SubTest1"},
                                    new SubInType {Name = "SubTest2"}
                                }
                            },
                            new SubInType {Name = "Test2"}
                        }
                },
                new InnTypeEnum {DateTimeValue = DateTime.Now, DesimalValue = (decimal) 100.212, StringValue = "test"}
            };
            return target;
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperTest()
        {
            var map = MapFactory.CreateMapRule<InnTypeEnum, OutTypeEnum>();
            Assert.AreEqual(2, map.Rules.Count);

        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperDictionaryTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();

            map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            Assert.AreEqual(2, map.Rules.Count);
            Assert.IsTrue(map.Rules.First().Value.Fields.First().Value.Convertible);

        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperDictionaryConvertTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            var target = GetDictionaryTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var firstTime = timer.ElapsedMilliseconds;
            timer.Reset();
            result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var lastTime = timer.ElapsedMilliseconds;
            Assert.AreEqual(1, result.YourDictionary.Count);
            Assert.AreEqual("SubTest1", result.YourDictionary.First().Value.Child.First().Name);
            Assert.AreEqual(2, result.StringToType.Count);
            Assert.AreEqual(2, result.TypeToString.Count);
        }

        private static DictionaryTestInn GetDictionaryTarget()
        {
            MapFactory.ResetAllMaps();
            var target = new DictionaryTestInn
            {
                MyDictionary = new Dictionary<string, string>
                {
                    {"test", "test"}
                },
                YourDictionary = new Dictionary<string, SubInType>
                {
                    {
                        "test", new SubInType
                        {
                            Name = "Jonas",
                            Child = new List<SubInType>
                            {
                                new SubInType {Name = "SubTest1"},
                                new SubInType
                                {
                                    Name = "SubTest2",
                                    Child = new List<SubInType>
                                    {
                                        new SubInType {Name = "SubTest1"},
                                        new SubInType {Name = "SubTest2"}
                                    }
                                }
                            }
                        }
                    }
                },
                StringToType =
                    new Dictionary<string, string> { { typeof(string).FullName, "string" }, { typeof(int).FullName, "int" } },
                TypeToString = new Dictionary<Type, string> { { typeof(string), "string" }, { typeof(int), "int" } },
                AddTest1 = "Jonas",
                FieldToFlatten = new FlatteningInput { FirstName = "Jonas", LastName = "Syrstad" },
                GivenName = "Jonas",
                SurName = "Syrsad"

            };
            return target;
        }


        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void TypeTypeConverterTest()
        {
            MapFactory.ResetAllMaps();
            try
            {
                TypeDescriptor.AddAttributes(typeof(Type), new[] { new TypeConverterAttribute(typeof(TypeStringTypeConverter)) });
            }
            catch (Exception)
            {


            }
            var stringRep = (string)TypeDescriptor.GetConverter(typeof(Type)).ConvertTo(typeof(string), typeof(string));
            Assert.AreEqual(typeof(string).FullName, stringRep);
            var type = (Type)TypeDescriptor.GetConverter(typeof(Type)).ConvertFrom(typeof(int).FullName);
            Assert.AreEqual(typeof(int), type);
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperRemoveNodeDictionaryConvertTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            map.GetRule<DictionaryTestInn, DictionaryTestOut>().RemoveMapping("TypeToString");
            var target = GetDictionaryTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var firstTime = timer.ElapsedMilliseconds;
            timer.Reset();
            result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var lastTime = timer.ElapsedMilliseconds;
            if (lastTime > firstTime && lastTime != firstTime)
                Assert.Fail("The first run took more time than the last {0}>{1}", lastTime, firstTime);
            Assert.AreEqual(1, result.YourDictionary.Count);
            Assert.AreEqual("SubTest1", result.YourDictionary.First().Value.Child.First().Name);
            Assert.AreEqual(2, result.StringToType.Count);
            Assert.IsNull(result.TypeToString);

        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperAddNodeDictionaryConvertTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            map.GetRule<DictionaryTestInn, DictionaryTestOut>()
                .RemoveMapping("TypeToString")
                .Add("AddTest1", "AddTest2", BasicTypes.Convertable);
            var target = GetDictionaryTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var firstTime = timer.ElapsedMilliseconds;
            timer.Reset();
            result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var lastTime = timer.ElapsedMilliseconds;
            if (lastTime > firstTime && lastTime != firstTime)
                Assert.Fail("The first run took more time than the last {0}>{1}", lastTime, firstTime);
            Assert.AreEqual(1, result.YourDictionary.Count);
            Assert.AreEqual("SubTest1", result.YourDictionary.First().Value.Child.First().Name);
            Assert.AreEqual(2, result.StringToType.Count);
            Assert.IsNull(result.TypeToString);
            Assert.AreEqual("Jonas", result.AddTest2);

        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperAddFlatteningNodeDictionaryConvertTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            map.GetRule<DictionaryTestInn, DictionaryTestOut>()
                .RemoveMapping("TypeToString")
                .Add("FieldToFlatten", "this", BasicTypes.FlatenComplexType);
            map.AddMap<FlatteningInput, DictionaryTestOut>();
            var target = GetDictionaryTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var firstTime = timer.ElapsedMilliseconds;
            timer.Reset();
            result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var lastTime = timer.ElapsedMilliseconds;
            if (lastTime > firstTime && lastTime != firstTime)
                Assert.Fail("The first run took more time than the last {0}>{1}", lastTime, firstTime);
            Assert.AreEqual(1, result.YourDictionary.Count);
            Assert.AreEqual("SubTest1", result.YourDictionary.First().Value.Child.First().Name);
            Assert.AreEqual(2, result.StringToType.Count);
            Assert.IsNull(result.TypeToString);
            Assert.AreEqual("Jonas", result.FirstName);

        }
        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void AutoMapperAddExpanderNodeDictionaryConvertTest()
        {
            MapFactory.ResetAllMaps();
            var map = MapFactory.CreateMapRule<DictionaryTestInn, DictionaryTestOut>();
            map.GetRule<DictionaryTestInn, DictionaryTestOut>()
                .RemoveMapping("TypeToString")
                .Add("GivenName", "ExpanderField", BasicTypes.Expander);
            map.AddMap<DictionaryTestInn, FlatteningInput>(new Dictionary<string, string> { { "GivenName", "FirstName" }, { "SurName", "LastName" } });
            var target = GetDictionaryTarget();
            var timer = Stopwatch.StartNew();
            var result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var firstTime = timer.ElapsedMilliseconds;
            timer.Reset();
            result = target.Map().To<DictionaryTestOut>(map);
            Assert.AreEqual(1, result.MyDictionary.Count);
            var lastTime = timer.ElapsedMilliseconds;
            if (lastTime > firstTime && lastTime != firstTime)
                Assert.Fail("The first run took more time than the last {0}>{1}", lastTime, firstTime);
            Assert.AreEqual(1, result.YourDictionary.Count);
            Assert.AreEqual("SubTest1", result.YourDictionary.First().Value.Child.First().Name);
            Assert.AreEqual(2, result.StringToType.Count);
            Assert.IsNull(result.TypeToString);
            Assert.AreEqual("Jonas", result.ExpanderField.FirstName);

        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void DictionaryBaseTest()
        {
            var map = MapFactory.CreateMapRule<InnType, OutType2>();

            //Adding mapping rule for field that does not match on name.
            map.GetRule<InnType, OutType2>()
                .Add(type => type.DesimalValue, type => type.DesimalValue1, BasicTypes.Convertable);
            var target = new Dictionary<int, InnType>
            {
                {
                    1,
                    new InnType
                    {
                        DateTimeValue = DateTime.Now,
                        MyInts = new List<int>(),
                        StringValue = "test1",
                        DesimalValue = 1
                    }
                },
                {
                    2,
                    new InnType
                    {
                        DateTimeValue = DateTime.Now,
                        MyInts = new List<int>(),
                        StringValue = "test1",
                        DesimalValue = 2
                    }
                }                
            };
            var result = target.Map<int, InnType>().To<string, OutType2>(map);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void DottedSourceTest()
        {
            MapFactory.CreateMapRule<DottedSource, OutType>()
                .Add(s => s.Dotted.StringValue, d => d.StringValue1);
            var target = new DottedSource { Dotted = new InnType { StringValue = "test" } };
            var result = target.Map().To<OutType>();
            Assert.AreEqual("test", result.StringValue1);
        }

        [TestMethod()]
        [TestCategory("Type Mapper")]
        public void MergeDataWithSameBaseType()
        {
            MapFactory.CreateMapRule<InnType, InnType>(true).GetRule().RemoveMapping("MyInts").RemoveMapping("DesimalValue");
            var inValue = new InnType
            {
                DateTimeValue = DateTime.Now,
                MyInts = new List<int>(),
                StringValue = "Updated string",
                DesimalValue = 100
            };
            var targetValue = new InnType
            {
                DateTimeValue = DateTime.Now.AddYears(-1),
                MyInts = new List<int>(),
                StringValue = "original string",
                DesimalValue = 0
            };
            var newTarget = inValue.Map().To(targetValue);
            
            Assert.AreNotSame(inValue, newTarget);
            Assert.AreEqual(inValue.StringValue, newTarget.StringValue);
            Assert.AreEqual(inValue.DateTimeValue, newTarget.DateTimeValue);
            Assert.AreNotEqual(inValue.DesimalValue, newTarget.DesimalValue);

        }

    }

    public class StringInnType
    {
        public string Value1 { get; set; }

        public string Value2 { get; set; }

        public string Value3 { get; set; }

        public string Email { get; set; }
    }

    public class StringOutType1
    {
        public string Value1 { get; set; }

        public string Value2 { get; set; }

        public string Value3 { get; set; }

        public string Email { get; set; }
    }
}