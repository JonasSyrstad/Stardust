// TODO: make work again

//using Pragma.Core.Services;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using Pragma.Core.FactoryHelpers;
//using Pragma.Core.Services.Tasks;
//using System.Data.Objects;
//using Pragma.Core.Services.Workflow;

//namespace Pragma.Core.ServicesTest
//{


//    /// <summary>
//    ///This is a test class for RuntimeHelpersTest and is intended
//    ///to contain all RuntimeHelpersTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class RuntimeHelpersTest
//    {


//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
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

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion


//        /// <summary>
//        ///A test for GetStateStorageContainer
//        ///</summary>
//        [TestMethod()]
//        public void GetStateStorageContainerTest()
//        {
//            try
//            {
//                var runtime = RuntimeFactory.CreateRuntime(Scope.PerRequest);
//                var transaction = runtime.GetServiceTransaction(Scope.PerRequest);
//                transaction.CreateTransaction();
//                var sqlConn = runtime.GetSqlConnection(Scope.PerRequest);
//                sqlConn.GetConnection(transaction.GetTransaction());
//                sqlConn.GetConnection().Close();
//                transaction.Commit();
//            }
//            catch (Exception ex)
//            {

//                Assert.Fail(ex.Message);
//            }

//        }

//        /// <summary>
//        ///A test for GetSqlConnection
//        ///</summary>
//        [TestMethod()]
//        public void GetSqlConnectionTest()
//        {
//            IRuntime self = RuntimeFactory.CreateRuntime(Scope.PerRequest);
//            ITransactedSqlConnection actual;
//            actual = self.GetSqlConnection(Scope.PerRequest);
//            Assert.IsNotNull(actual);
//        }

//        /// <summary>
//        ///A test for GetEntityContext
//        ///</summary>
//        public void GetEntityContextTestHelper<T>()
//            where T : ObjectContext
//        {
//            IRuntime self = null; // TODO: Initialize to an appropriate value
//            Scope scope = new Scope(); // TODO: Initialize to an appropriate value
//            ITransactedEfContext<T> expected = null; // TODO: Initialize to an appropriate value
//            ITransactedEfContext<T> actual;
//            actual = self.GetEntityContext<T>(scope);
//            Assert.AreEqual(expected, actual);
//            Assert.Inconclusive("Verify the correctness of this test method.");
//        }

//        [TestMethod()]
//        public void GetEntityContextTest()
//        {
//            Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
//                    "Please call GetEntityContextTestHelper<T>() with appropriate type parameters.");
//        }
//    }

//    public static class Helpers
//    {
//        public static ITransactedSqlConnection GetSqlConnection(this IRuntime self, Scope scope = Scope.Context)
//        {
//            return self.CreateRuntimeTask<ITransactedSqlConnection>(scope);
//        }

//        public static ITransactedEfContext<T> GetEntityContext<T>(this IRuntime self, Scope scope = Scope.Context) where T : ObjectContext
//        {
//            return self.CreateRuntimeTask<ITransactedEfContext<T>>(scope, typeof(T));
//        }
//    }
//}
