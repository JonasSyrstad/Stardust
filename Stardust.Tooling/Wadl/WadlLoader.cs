//using System;
//using System.Net;
//using System.Xml;
//using System.Xml.Schema;
//using System.Xml.Serialization;

//namespace Stardust.Stardust_Tooling.Wadl
//{
//    public class WadlLoader
//    {
//        static void RecurseExternals(XmlSchema schema)
//        {
//            foreach (XmlSchemaExternal external in schema.Includes)
//            {
//                if (external.SchemaLocation != null)
//                {
//                    Console.WriteLine("External SchemaLocation: {0}", external.SchemaLocation);
//                }

//                if (external is XmlSchemaImport)
//                {
//                    XmlSchemaImport import = external as XmlSchemaImport;
//                    Console.WriteLine("Imported namespace: {0}", import.Namespace);
//                }

//                if (external.Schema != null)
//                {
//                    external.Schema.Write(Console.Out);
//                    RecurseExternals(external.Schema);
//                }
//            }
//        }

//        static void ValidationCallback(object sender, ValidationEventArgs args)
//        {
//            if (args.Severity == XmlSeverityType.Warning)
//                Console.Write("WARNING: ");
//            else if (args.Severity == XmlSeverityType.Error)
//                Console.Write("ERROR: ");

//            Console.WriteLine(args.Message);
//        }

//        public static WadlDto Load(Uri location)
//        {
//            try
//            {
//                WadlExtensions.WriteLine("Getting wadl");
//                var schemas = new WadlDto();
//                using (var client = new WebClient())
//                {
//                    XmlSchemaSet schemaSet = new XmlSchemaSet();

//                    var xdoc = new XmlDocument();
//                    xdoc.Load(client.OpenRead(location));
//                    var serializer = new XmlSerializer(typeof(Application));
//                    schemas.Wadl = (Application)serializer.Deserialize(client.OpenRead(location));
//                    schemas.Wadl.Attributes = new XmlAttribute[xdoc.LastChild.Attributes.Count];
//                    xdoc.LastChild.Attributes.CopyTo(schemas.Wadl.Attributes, 0);
//                    foreach (var gramar in schemas.Wadl.Grammars.Include)
//                    {
//                        WadlExtensions.WriteLine(string.Format("Getting schema: '{0}'", gramar.Href));
//                        var s = XmlSchema.Read(client.OpenRead(new Uri(gramar.Href)), (sender, e) => { });
//                        schemas.Includes.Add(s.TargetNamespace.ToLower(), s);
//                        GetIncludes(s, client, schemas);
//                    }
//                }
//                WadlExtensions.WriteLine("Compiling schemas");
//                schemas.Compile();
//                WadlExtensions.WriteLine(string.Format("wadl and {0} schemas compiled", schemas.Wadl.Grammars.Include.Count));
//                WadlExtensions.CreatedClasses.Clear();
//                return schemas;
//            }
//            catch (Exception ex)
//            {

//                throw;
//            }
//        }

//        private static void GetIncludes(XmlSchema schema, WebClient client, WadlDto schemas)
//        {

//            foreach (var o in schema.Includes)
//            {
//                var include = (XmlSchemaImport)o;
//                try
//                {
//                    var s = XmlSchema.Read(client.OpenRead(include.SchemaLocation), (sender, e) => { });
//                    if (!schemas.Includes.ContainsKey(s.TargetNamespace))
//                    {
//                        schemas.Includes.Add(s.TargetNamespace, s);
//                        if (s.Includes != null && s.Includes.Count != 0)
//                        {
//                            GetIncludes(s, client, schemas);
//                        }
//                    }
//                }
//                catch (Exception)
//                {
//                }
//            }
//        }
//    }
//}