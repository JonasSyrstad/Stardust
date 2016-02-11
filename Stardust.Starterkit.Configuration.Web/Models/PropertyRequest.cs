namespace Stardust.Starterkit.Configuration.Web.Models
{
    public class PropertyRequest
    {
        public string Environment { get; set; }

        public string PropertyName { get; set; }

        public VariableTypes Type { get; set; }

        public string ParentContainer { get; set; }

        public string Value { get; set; }

        public string ParentFormatString { get; set; }

        public bool IsSecure { get; set; }

        public string SubContainer { get; set; }
    }
}