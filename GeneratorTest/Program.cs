using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stardust.Stardust_Tooling;

namespace GeneratorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var gen=new Generator();
            var file = gen.CreateFile("Stardust.Test", File.ReadAllText(@"c:\temp\config.json"));
            Console.WriteLine(file);
        }
    }

}
