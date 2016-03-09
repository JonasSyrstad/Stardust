

using Stardust.Interstellar.Utilities;
using Stardust.Particles;

namespace Stardust.Frontier.BatchProcessor
{
    public interface IBatchEnviromentResolver
    {
        EnvironmentDefinition GetEnvironment();
    }

    class BatchEnviromentResolver : IBatchEnviromentResolver
    {
        public EnvironmentDefinition GetEnvironment()
        {
            return new EnvironmentDefinition {Name= Utilities.GetEnvironment()};
        }
    }
}