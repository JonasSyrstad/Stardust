namespace Stardust.Nucleus.ContextProviders
{
    public interface IControlledProvider
    {
        string ScopeId { get; }

        IScopeProvider EndScope();

        IExtendedScopeProvider EndCurrentAndBeginNew();
    }
}