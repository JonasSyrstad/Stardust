using BrightstarDB.Client;
using BrightstarDB.Storage;

namespace Stardust.Core.Sandbox
{
    public class BrightstarTest
    {
        private IBrightstarService Client = BrightstarService.GetClient();

        public BrightstarTest()
        {
            Client.CreateStore("test", PersistenceType.Rewrite);
            
        }

    }
}
