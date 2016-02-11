namespace Stardust.Core.Default.Implementations
{
    internal interface IIgnoreAttribute
    {
        bool IsIgnored(string memberName);
    }
}