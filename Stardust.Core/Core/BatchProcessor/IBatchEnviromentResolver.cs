

using Stardust.Particles;

namespace Stardust.Core.BatchProcessor
{
    public interface IBatchEnviromentResolver
    {
        EnvironmentDefinition GetEnvironment();
    }

    class BatchEnviromentResolver : IBatchEnviromentResolver
    {
        public EnvironmentDefinition GetEnvironment()
        {
            return new EnvironmentDefinition {Name= ConfigurationManagerHelper.GetValueOnKey("environment")};
        }
    }
}