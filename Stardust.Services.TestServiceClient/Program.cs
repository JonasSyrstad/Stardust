using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Nucleus;

namespace Pragma.Services.TestServiceClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Resolver.GetConfigurator().Bind<IConfigurationReader>().To<JsonConfigurationReader>();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
