using System;
using System.Threading.Tasks;
using Xunit;

namespace Stardust.Interstellar.Rest.Test
{
    public class ProxyGeneratorTest
    {
        [Fact]
        public async Task GeneratorTest()

        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/api");
            try
            {
                var res =await service.ApplyAsync("test", "Jonas Syrstad", "Hello", "Sample");
                Console.WriteLine(res);
            }
            catch (Exception ex)
            {

            }
            try
            {
                await service.PutAsync("test",DateTime.Today);
            }
            catch (Exception ex)
            {

            }
        }
    }
}