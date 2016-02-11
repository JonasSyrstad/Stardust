using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class CacheSettings
    {
        public string CacheType
        {
            get
            {
                if (CacheImplementation.IsNullOrWhiteSpace()) return "default";
                return CacheImplementation;
            }
        }
    }
}