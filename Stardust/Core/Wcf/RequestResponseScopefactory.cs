namespace Stardust.Core.Wcf
{
    public static class RequestResponseScopefactory
    {
        public static IStardustContext CreateScope()
        {
            return ContextScopeExtensions.CreateScope();
        }
    }
}