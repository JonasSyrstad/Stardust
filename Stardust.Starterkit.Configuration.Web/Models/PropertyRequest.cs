namespace Stardust.Starterkit.Configuration.Web.Models
{
    public class PropertyRequest
    {
        private string _propertyName;
        private string environment;
        private string _parentContainer;
        private string _subContainer;

        public string Environment

        {
            get { return environment?.Trim(); }
            set { environment = value?.Trim(); }
        }

        public string PropertyName
        {
            get
            {
                return _propertyName?.Trim();
            }
            set {
                _propertyName = value?.Trim();
            }
        }

        public VariableTypes Type { get; set; }

        public string ParentContainer
        {
            get { return _parentContainer?.Trim(); }
            set { _parentContainer = value?.Trim(); }
        }

        public string Value { get; set; }

        public string ParentFormatString { get; set; }

        public bool IsSecure { get; set; }

        public string SubContainer
        {
            get { return _subContainer?.Trim(); }
            set { _subContainer = value?.Trim(); }
        }

        public string Description { get; set; }
    }
}