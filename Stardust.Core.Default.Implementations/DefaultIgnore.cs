namespace Stardust.Core.Default.Implementations
{
    class DefaultIgnore: IIgnoreAttribute
    {
        bool IIgnoreAttribute.IsIgnored(string memberName)
        {
            return false;
        }
    }
}