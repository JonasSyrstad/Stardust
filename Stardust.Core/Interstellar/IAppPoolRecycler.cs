namespace Stardust.Interstellar
{
    /// <summary>
    /// Implement this to properly recycle IIS app pools on your platform.
    /// </summary>
    public interface IAppPoolRecycler
    {
        bool TryRecycleCurrent();

        bool TryRecycle(string appPoolId);

        bool TryRecycleAll();
    }
}