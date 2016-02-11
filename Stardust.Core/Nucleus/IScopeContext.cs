namespace Stardust.Nucleus
{
    public interface IScopeContext
    {
        IScopeContext SetRequestResponseScope();

        IScopeContext SetTransientScope();

        IScopeContext SetSessionScope();

        IScopeContext SetThreadScope();

        IScopeContext SetSingletonScope();

        /// <summary>
        /// Contains the current activation scope for the registration
        /// </summary>
        Scope? ActivationScope { get; }

        /// <summary>
        /// Set to true if the client code is allowed to provide it's own scope
        /// </summary>
        bool AllowOverride { get; set; }

        bool IsNull { get; }
        string ImplementationKey { get; }

        void SetAllowOverride();
        void DisableOverride();
    }
}