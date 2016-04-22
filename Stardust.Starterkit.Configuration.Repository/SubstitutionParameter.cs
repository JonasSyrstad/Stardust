using System.Linq;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class SubstitutionParameter
    {
        public string Value
        {
            get
            {
                if (ItemValue == null)
                    return Parent == null ? "" : Parent.Value;
                return ItemValue;
            }
        }

        public bool IsRoot { get { return Parent == null; }}

        public bool IsInherited
        {
            get { return string.IsNullOrWhiteSpace(ItemValue) && !IsRoot; }
        }

        public string ViewName
        {
            get
            {
                if (HostParameters != null && HostParameters.Any()) return HostParameters.Select(s => s.Name).FirstOrDefault();
                if (EndpointParameters != null && EndpointParameters.Any()) return EndpointParameters.Select(s => s.Name).FirstOrDefault();
                return null;
            }
        }

        public override string ToString()
        {
            return Id;
        }
    }
}