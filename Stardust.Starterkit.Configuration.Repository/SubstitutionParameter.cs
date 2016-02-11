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

        public override string ToString()
        {
            return Id;
        }
    }
}