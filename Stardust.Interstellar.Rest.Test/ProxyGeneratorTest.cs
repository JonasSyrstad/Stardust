using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Stardust.Interstellar.Rest.Test
{
    public class ProxyGeneratorTest
    {
        private readonly ITestOutputHelper output;

        public ProxyGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public async Task GeneratorTest()

        {
            var service = ProxyFactory.CreateInstance<ITestApi>("http://localhost/Stardust.Interstellar.Test/api");
            try
            {
                var res =await service.ApplyAsync("test", "Jonas Syrstad", "Hello", "Sample");
                output.WriteLine(res);
            }
            catch (Exception ex)
            {
                throw;
            }
            try
            {
                await service.PutAsync("test",DateTime.Today);
                output.WriteLine("Put was successfull");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}