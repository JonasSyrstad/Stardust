/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Stardust.Stardust_Tooling.Wadl
{
    [XmlRoot(ElementName = "include", Namespace = "http://www.w3.org/2001/XMLSchema")]
    public class Include
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "grammars", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Grammars
    {
        [XmlElement(ElementName = "include", Namespace = "http://www.w3.org/2001/XMLSchema")]
        public List<Include> Include { get; set; }
    }

    [XmlRoot(ElementName = "representation", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Representation
    {
        [XmlAttribute(AttributeName = "mediaType")]
        public string MediaType { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlRoot(ElementName = "request", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Request
    {
        [XmlElement(ElementName = "representation", Namespace = "http://wadl.dev.java.net/2009/02")]
        public List<Representation> Representation { get; set; }
    }

    [XmlRoot(ElementName = "response", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Response
    {
        [XmlElement(ElementName = "representation", Namespace = "http://wadl.dev.java.net/2009/02")]
        public List<Representation> Representation { get; set; }
    }

    [XmlRoot(ElementName = "method", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Method
    {
        [XmlElement(ElementName = "request", Namespace = "http://wadl.dev.java.net/2009/02")]
        public Request Request { get; set; }
        [XmlElement(ElementName = "response", Namespace = "http://wadl.dev.java.net/2009/02")]
        public Response Response { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "resource", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Resource
    {
        [XmlElement(ElementName = "method", Namespace = "http://wadl.dev.java.net/2009/02")]
        public Method Method { get; set; }
        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
    }

    [XmlRoot(ElementName = "resources", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Resources
    {
        [XmlElement(ElementName = "resource", Namespace = "http://wadl.dev.java.net/2009/02")]
        public List<Resource> Resource { get; set; }
        [XmlAttribute(AttributeName = "base")]
        public string Base { get; set; }
    }

    [XmlRoot(ElementName = "application", Namespace = "http://wadl.dev.java.net/2009/02")]
    public class Application
    {

        [XmlElement(ElementName = "grammars", Namespace = "http://wadl.dev.java.net/2009/02")]
        public Grammars Grammars { get; set; }
        
        [XmlElement(ElementName = "resources", Namespace = "http://wadl.dev.java.net/2009/02")]
        public Resources Resources { get; set; }
        
        [XmlIgnore]
        public XmlAttribute[] Attributes { get; set; }

        [XmlIgnore]
        public string Name {
            get
            {
                var name = "name";
                return GetAttributeByName(name);
            }
        }

        private string GetAttributeByName(string name)
        {
                return Attributes.Where(a => a.Name == name).Select(a => a.Value).Single();
        }
    }

}
