using System.ComponentModel;

namespace Stardust.Starterkit.Configuration.Web.Controllers
{
    [DisplayName("Access key")]
    public class ReaderKey 
    {
        [DisplayName("Access key")]
        public string Key { get; set; }

        [DisplayName("Allow clients to use master key")]
        public bool AllowMaster { get; set; }

        [DisplayName("Generate new key")]
        public bool GenerateNew { get; set; }
    }
}