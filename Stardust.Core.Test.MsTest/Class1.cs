using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Nucleus.ContextProviders;

namespace Stardust.Core.Test.MsTest
{
    public class TestContextScopeProvider : ScopeProviderBase
    {
        private static TestContext Context;

        protected override void DoBind(Type type, object toInstance)
        {
            Context.Properties.Add(type,toInstance);
        }

        public override void InvalidateBinding(Type type)
        {
            Context.Properties.Remove(type);
        }

        public override void KillEmAll()
        {
            Context.Properties.Clear();
        }

        protected override object DoResolve(Type type)
        {
            return Context.Properties.Contains(type) ? Context.Properties[type] : null;
        }

        public static void InitalizeTest(TestContext context)
        {
            Context = context;
        }

        public static void EndTest()
        {
            Context = null;
        }
    }
}
