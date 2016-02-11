using System;
using System.Collections.Generic;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.Mock
{
    public class StringClass
    {
        public string header1 { get; set; }
        public string header2 { get; set; }
        public string Header3 { get; set; }

        public void header3(string value)
        {
            Header3 = value;
        }
        public string header4;
    }

    public class TypedClass
    {
        public int header1 { get; set; }
        public string header2 { get; set; }
        public decimal header3 { get; set; }
        public string header4 { get; set; }
    }

    public class DottedSource
    {
        public InnType Dotted { get; set; }
    }

    public class InnType
    {
        public string StringValue { get; set; }
        public DateTime DateTimeValue { get; set; }

        public decimal DesimalValue { get; set; }

        public List<int> MyInts { get; set; }

        public bool MyBoolean { get; set; }

        public MyEnum MyEnum { get; set; }
    }
    public class OutType
    {
        public string StringValue1 { get; set; }
        public string DateTimeValue1 { get; set; }

        public decimal DesimalValue1 { get; set; }

        public IEnumerable<string> MyInts { get; set; }

        public string MyBoolean { get; set; }

        public int MyEnumVal { get; set; }

        public string MyEnumName { get; set; }
    }

    public enum MyEnum
    {
        test,
        unit,
        something
    }

    public class OutType2
    {
        public string StringValue { get; set; }
        public DateTime DateTimeValue { get; set; }

        public decimal DesimalValue1 { get; set; }

        public IEnumerable<string> MyInts { get; set; }

        public bool MyBoolean { get; set; }

        public string MyEnum { get; set; }
    }

    public class InnTypeEnum
    {
        public string StringValue { get; set; }
        public DateTime DateTimeValue { get; set; }

        public decimal DesimalValue { get; set; }

        public IEnumerable<SubInType> Child { get; set; }
    }

    public class OutTypeEnum
    {
        public string StringValue { get; set; }
        public DateTime DateTimeValue { get; set; }

        public decimal DesimalValue { get; set; }

        public IEnumerable<SubOutType> Child { get; set; }
    }

    public class SubInType
    {
        public string Name { get; set; }
        public IEnumerable<SubInType> Child { get; set; }
    }

    public class SubOutType
    {
        public string Name { get; set; }
        public IEnumerable<SubOutType> Child { get; set; }

    }

    public class DictionaryTestInn
    {
        public Dictionary<string, string> MyDictionary { get; set; }
        public Dictionary<string, SubInType> YourDictionary { get; set; }

        public Dictionary<string, string> StringToType { get; set; }

        public Dictionary<Type, string> TypeToString { get; set; }

        public string AddTest1 { get; set; }

        public FlatteningInput FieldToFlatten { get; set; }
       



        public string SurName { get; set; }

        public string GivenName { get; set; }
    }

    public class FlatteningInput
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class DictionaryTestOut
    {
        public Dictionary<string, string> MyDictionary { get; set; }
        public Dictionary<string, SubOutType> YourDictionary { get; set; }

        public Dictionary<Type, string> StringToType { get; set; }

        public Dictionary<string, string> TypeToString { get; set; }

        public string AddTest2 { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public FlatteningInput ExpanderField { get; set; }
    }
}
