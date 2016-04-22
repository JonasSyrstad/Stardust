using System.Collections.Generic;
using System.Linq;
using System.Web.Management;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Web.Models
{
    public class SubstitutionParameterModel
    {
        public string EnvironmentNameId { get; set; }
        public string Id { get; set; }
        public bool IsInherited { get; set; }
        public bool IsRoot { get; set; }
        public string Value { get; set; }
        public string ItemValue { get; set; }
        public string Name { get; set; }

        public string ViewName { get; set; }

        public int Type { get { return Category == "Common" ? 1 : 2; } }

        public string DisplayName
        {
            get
            {
                if (ViewName != null) return ViewName;
                if (Name.Contains("_"))
                {
                    var val = Name.Split('_');
                    if(val.Length>2)
                        return string.Join("_",val.Skip(1));
                    return val.Last();
                }
                return Name;
            }
        }

        public string SortCategory
        {
            get
            {
                if (Name.Contains("_"))
                {
                    if (ViewName.ContainsCharacters()) return "2" + Name.Replace("_"+ViewName, "");
                    var val = Name.Split('_');
                    if (val.Length == 2)
                        return "2 " + val[0];
                    if (val.Length > 2)
                    {
                        var data = new List<string>();
                        for (int i = 0; i < val.Length - 1; i++)
                        {
                            data.Add(val[i]);
                        }
                        return "2 "+string.Join("_", data);
                    }
                }

                return "1 Common";
            }
        }
        public string Category
        {
            get
            {
                if (Name.Contains("_"))
                {
                    var val = Name.Split('_');
                    if (val.Length == 2)
                        return val[0];
                    if (val.Length > 2)
                    {
                        var data = new List<string>();
                        for (int i = 0; i < val.Length - 1; i++)
                        {
                            data.Add(val[i]);
                        }
                        return string.Join("_", data);
                    }
                }
                
                return "Common";
            }
        }
    }
}