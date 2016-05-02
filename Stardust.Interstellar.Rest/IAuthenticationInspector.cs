namespace Stardust.Interstellar.Rest
{
    public interface IAuthenticationInspector
    {
        IAuthenticationHandler GetHandler();
    }
}