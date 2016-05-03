using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Stardust.Interstellar.Config;

namespace LoadTest
{
    class Program
    {
        
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidation;
            var config= Configuration.GetConfiguration();
            var env=config.Environments.First();
            //new BenchmarkTest().TestExecution();
            //Console.ReadLine();
        }

        private static bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
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
