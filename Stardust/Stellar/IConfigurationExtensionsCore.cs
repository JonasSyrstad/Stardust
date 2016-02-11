namespace Stardust.Stellar
{
    public interface IConfigurationExtensionsCore
    {
        string this[string name] { get; }

        string Secure(string name);
    }
}