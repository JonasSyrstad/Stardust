using System;

namespace LoadTest
{
    class Program
    {
        
        static void Main(string[] args)
        {
            
            new BenchmarkTest().TestExecution();
            Console.ReadLine();
        }

        
    }
    #region Test classes

    interface ITestClass3
    {
        ITestClass2 TClass { get; set; }

        string MyString2 { get; set; }
    }

     class TestClass3 : ITestClass3
    {
        public TestClass3(ITestClass2 testclass)
        {
            TClass = testclass;
            MyString2 = testclass.GetType().FullName;
        }
        public ITestClass2 TClass { get; set; }

        public string MyString2 { get; set; }
    }

     interface ITestClass2
    {
        string MyString1 { get; set; }
        string MyString2 { get; set; }
    }

    class TestClass2 : ITestClass2
    {
        public TestClass2()
        {
        }

        public string MyString1 { get; set; }

        public string MyString2 { get; set; }
    }
     interface ITestClass
    {
        string MyString1 { get; set; }
        string MyString2 { get; set; }
    }

    class TestClass : ITestClass
    {
        public TestClass()
        {
            //MyString1 = Guid.NewGuid().ToString();
        }

        public string MyString1 { get; set; }

        public string MyString2 { get; set; }
    }
    #endregion
}
