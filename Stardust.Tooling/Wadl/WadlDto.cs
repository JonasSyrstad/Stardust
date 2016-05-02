using System.Collections.Generic;
using System.Xml.Schema;

namespace Stardust.Stardust_Tooling.Wadl
{
    public class WadlDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WadlDto(Application wadl, Dictionary<string, XmlSchema> includes)
        {
            Wadl = wadl;
            Includes = includes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WadlDto()
        {
            Includes = new Dictionary<string, XmlSchema>();
        }

        public Application Wadl { get; set; }

        public Dictionary<string, XmlSchema> Includes { get; set; }

        public XmlSchema Schema { get; set; }
    }
}
