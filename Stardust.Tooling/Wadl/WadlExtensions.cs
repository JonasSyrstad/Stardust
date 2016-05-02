//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Xml.Schema;
//using Microsoft.VisualStudio.Shell.Interop;
//using Stardust.Accretion;

//namespace Stardust.Stardust_Tooling.Wadl
//{
//    public static class WadlExtensions
//    {
//        public static void EnableOutputWindow(IVsOutputWindowPane outWindow)
//        {
//            output = outWindow;
//        }

//        public static void WriteLine(string message)
//        {
//            if (output != null) output.OutputString(string.Format("{0}: {1}{2}", DateTime.Now, message, System.Environment.NewLine));
//        }

//        public static string ToProperCase(this string text)
//        {
//            var myTi = new CultureInfo("en-US", false).TextInfo;
//            return myTi.ToTitleCase(text.ToLower());
//        }

//        public static WadlDto SetWcfNamespace(this WadlDto wadl, string generatorNamespace)
//        {
//            wcfNs = generatorNamespace;
//            return wadl;
//        }

//        public static WadlDto SetWcfDataContractNamespaceRoot(this WadlDto wadl, string generatorNamespace)
//        {
//            datacontractNs = generatorNamespace;
//            return wadl;
//        }

//        public static string CreateServiceContract(this WadlDto wadl, string generatorNamespace = null)
//        {
//            if (string.IsNullOrWhiteSpace(generatorNamespace)) generatorNamespace = string.Format("TERS.Generator.ServiceModels.{0}", wadl.Wadl.Name);
//            WriteLine("Building header, usings and namespace");
//            var writer = WriterFactory.CreateWriter();
//            writer.AddGeneratorHeader()
//                .Using("System")
//                .Using("System.CodeDom.Compiler")
//                .Using("System.ComponentModel")
//                .Using("System.Diagnostics")
//                .Using("System.Globalization")
//                .Using("System.Collections.Generic")
//                .Using("System.Runtime.Serialization")
//                .Using("System.ServiceModel")
//                .Using("System.Threading.Tasks.Task")
//                .Using("Newtonsoft.Json")
//                .Using("Newtonsoft.Json.Serialization")
//                .DeclareNamespace(
//                    generatorNamespace,
//                    w =>
//                        {
//                            //if(wadl.Wadl.)
//                            //w.Attribute("");
//                            w.Interface(
//                                wadl.Wadl.Name.ToProperCase(),
//                                serviceBody =>
//                                    {
//                                        foreach (var resource in wadl.Wadl.Resources.Resource)
//                                        {
//                                            var type = wadl.GetSchemaType(resource);
//                                            var schema = wadl.Includes[type.ToLower()];
//                                            var inputs = schema.GetTypes(resource.Method.Request.Representation);
//                                            var output = schema.GetType(resource.Method.Response.Representation);
//                                            var methodName = resource.Path.Replace("/", "").ToProperCase();
//                                            serviceBody.Attribute($"Route(\"{resource.Path}\")")
//                                            .IMethod(resource.Method.Name.ToProperCase(), inputsss, output);
//                                        }
//                                    });
//                        });
//            //    .BeginBlock();
//            //WriteLine("Creating interface");
//            //builder = wadl.BuildIn    terface(builder);
//            //foreach (var xmlSchema in wadl.Includes)
//            //{

//            //    BuildDataTypes(builder, xmlSchema.Value, GetPrefix(xmlSchema));
//            //}
//            //builder.EndBlock();
//            //return builder.ToString();
//        }



//        private static string GetPrefix(KeyValuePair<string, XmlSchema> xmlSchema)
//        {
//            return xmlSchema.Key.GetHashCode().ToString().Replace("-", "");
//        }

//        private static void BuildDataTypes(StringBuilder classBuilder, XmlSchema schema, string methodName)
//        {
//            foreach (XmlSchemaObject item in schema.Items)
//            {
//                CreateClass(classBuilder, methodName, item);
//            }
//        }

//        private static void CreateClass(StringBuilder classBuilder, string methodName, XmlSchemaObject item)
//        {
//            var element = item as XmlSchemaElement;
//            if (element != null)
//            {
//                WriteLine(string.Format("Building class {0}", element.Name));
//                BuildClassFromElement(classBuilder, methodName, element.SchemaType as XmlSchemaComplexType, element.Name);
//                WriteLine(string.Format("Class {0} built", element.Name));
//            }
//            var complexType = item as XmlSchemaComplexType;
//            if (complexType != null)
//            {
//                WriteLine(string.Format("Building class {0}", complexType.Name));
//                BuildClassFromElement(classBuilder, methodName, complexType, complexType.Name);
//                WriteLine(string.Format("Class {0} built", complexType.Name));
//            }
//        }

//        public static readonly List<string> CreatedClasses = new List<string>();
//        private static IVsOutputWindowPane output;

//        internal static string wcfNs;

//        internal static string datacontractNs;

//        private static void BuildClassFromElement(StringBuilder classBuilder, string methodName, XmlSchemaComplexType type, string name)
//        {
//            //var type = element.SchemaType as XmlSchemaComplexType;
//            if (type != null)
//            {
//                name = MakeClassName(methodName, name);
//                if (name.Contains("_"))
//                    name = name.ToProperCase();
//                if (CreatedClasses.Any(s => s == name))
//                    return;
//                CreatedClasses.Add(name);
//                classBuilder.AppendLine().AppendClassAttributes(System.Environment.Version.ToString())
//                    .AddDataContractAttribute(name, datacontractNs)
//                    .DeclareClass(name, true)
//                    .BeginBlock(1)
//                    .AddErrorHandler();
//                var particle = type.Particle as XmlSchemaGroupBase;
//                if (particle != null)
//                {
//                    var isFirst = true;
//                    foreach (var o in particle.Items)
//                    {
//                        if (!isFirst) classBuilder.AppendLine();
//                        isFirst = false;
//                        var props = (XmlSchemaElement)o;
//                        if (props.MaxOccursString == "unbounded")
//                        {
//                            var typename = GetTypeName(props);
//                            var memberName = GetPropertyName(props);
//                            classBuilder.CreateOracleArray(memberName, typename.ToProperCase());
//                        }
//                        else
//                        {
//                            var memberName = GetPropertyName(props);
//                            classBuilder.AppendLine(string.Format("\t\t[JsonProperty(\"{0}\")]", memberName));
//                            classBuilder.AppendLine(string.Format("\t\t[DataMember(Name = \"{0}\")]", memberName));
//                            var typeName = GetTypeName(props);
//                            if (typeName.Contains("_"))
//                                typeName = typeName.ToProperCase();
//                            if (memberName.Contains("_")) memberName = GetNormalizedMemerName(memberName);
//                            classBuilder.MakeProp(memberName, typeName);
//                        }

//                    }
//                }
//                else
//                {
//                    var s = "";
//                }
//                classBuilder.EndBlock(1);
//            }
//            else
//            {
//                var s = "";
//            }
//        }

//        public static StringBuilder AddDataContractAttribute(this StringBuilder classBuilder, string name, string ns)
//        {

//            return classBuilder.AppendLine(string.Format("\t[DataContract(Namespace=\"{1}{0}\")]", name, ns));
//        }

//        public static StringBuilder AddIndexerAttribute(this StringBuilder classBuilder)
//        {

//            return classBuilder.AppendLine("\t\t[IndexAttrubute(true, false, Analyze = true, Indexed = true, Save = true)]");
//        }

//        private static StringBuilder CreateOracleArray(this StringBuilder classBuilder, string memberName, string typename)
//        {
//            var orgName = memberName;
//            memberName = GetNormalizedMemerName(memberName);
//            return classBuilder.AppendLine(string.Format("\t\t[JsonProperty(\"{0}\")]", orgName))

//                .AppendLine("\t\t[EditorBrowsable(EditorBrowsableState.Never)]")
//                .AppendLine("\t\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]")
//                .AppendLine("\t\t[DebuggerHidden]")
//                .AppendLine(string.Format("\t\tprivate dynamic {0}hidden {{ get; set; }}", memberName))
//                .AppendLine()
//                .AppendLine("\t\t[EditorBrowsable(EditorBrowsableState.Never)]")
//                .AppendLine("\t\t[DebuggerBrowsable(DebuggerBrowsableState.Never)]")
//                .AppendLine(String.Format("\t\tprivate {0} {1}priv;", typename, memberName))
//                .AppendLine()
//                .AppendLine(string.Format("\t\t[JsonIgnore]", typename, memberName))
//                 .AppendLine(string.Format("\t\t[DataMember(Name = \"{0}\")]", orgName))
//                .AppendLine(string.Format("\t\tpublic {0} {1}", typename, memberName))
//                .BeginBlock(2)
//                .AppendLine(string.Format("\t\t\tget", typename, memberName))
//                .BeginBlock(3)
//                .AppendLine(string.Format("\t\t\t\tif ({1}priv != null) return {1}priv;", typename, memberName))
//                .AppendLine(string.Format("\t\t\t\tvar stringVal = (string){1}hidden.ToString();", typename, memberName))
//                .AppendLine(string.Format("\t\t\t\tif (stringVal.StartsWith(\"[\"))", typename, memberName))
//                .AppendLine(string.Format("\t\t\t\t\t{1}priv = ({0}) JsonConvert.DeserializeObject(stringVal, typeof({0}));", typename, memberName))
//                .AppendLine(string.Format("\t\t\t\telse", typename, memberName))
//                .AppendLine(string.Format("\t\t\t\t\t{1}priv = new {0}[1] {{ ({0})JsonConvert.DeserializeObject(stringVal, typeof({0})) }};", typename.Replace("[]", ""), memberName))
//                .AppendLine(string.Format("\t\t\t\treturn {1}priv;", typename, memberName))
//                .EndBlock(3)
//                .AppendLine(string.Format("\t\t\tset", typename, memberName))
//                .BeginBlock(3)
//                .AppendLine(string.Format("\t\t\t\t{1}hidden = {1}priv = value;", typename, memberName))
//                .EndBlock(3)
//                .EndBlock(2);
//        }

//        private static string GetNormalizedMemerName(string memberName)
//        {
//            if (memberName.Contains("_"))
//            {
//                memberName = memberName.ToProperCase();
//                if (memberName[1] == '_')
//                {
//                    memberName = memberName.Substring(2);
//                }
//                if (memberName.EndsWith("_"))
//                {
//                    memberName = memberName.Remove(memberName.Length - 1);
//                }
//            }
//            return memberName;
//        }

//        private static StringBuilder AddErrorHandler(this StringBuilder classBuilder)
//        {
//            return classBuilder.AppendLine("\t\t[OnError]")
//                .AppendLine("\t\t[EditorBrowsable(EditorBrowsableState.Never)]")
//                .AppendLine("\t\t[DebuggerHidden]")
//                .AppendLine("\t\tinternal void OnError(StreamingContext context, ErrorContext errorContext)")
//                .BeginBlock(2)
//                .AppendLine("\t\t\terrorContext.Handled = true;")
//                .EndBlock(2)
//                .AppendLine();
//        }

//        private static string MakeClassName(string methodName, string name)
//        {
//            name = name.Replace(".", "");
//            var prefix = "";
//            if (name.EndsWith("parameters", StringComparison.InvariantCultureIgnoreCase))
//            {
//                prefix = "_" + methodName;
//            }
//            name = name + prefix;
//            return name;
//        }

//        private static string GetPropertyName(XmlSchemaElement props)
//        {
//            if (!String.IsNullOrWhiteSpace(props.Name))
//                return props.Name;
//            return props.RefName.Name;
//        }

//        private static string GetTypeName(XmlSchemaElement props)
//        {
//            var name = GetPropertyNameRoot(props);
//            if (name.StartsWith("string")) return "string";
//            if (name.Equals("dateTime")) name = "DateTime";
//            if (name.EndsWith("parameters", StringComparison.InvariantCultureIgnoreCase))
//            {
//                var root = GetRoot(props) as XmlSchema;
//                if (root != null) return name + "_" + root.TargetNamespace.ToLower().GetHashCode().ToString().Replace("-", "");

//            }
//            if (props.MaxOccursString == "unbounded") return name + "[]";
//            if (props.MinOccurs == 0 && new[] { "DateTime", "long", "int", "decimal", "short", "float", "double" }.Contains(name)) return name + "?";

//            return name;
//        }

//        private static XmlSchemaObject GetRoot(XmlSchemaObject props)
//        {
//            while (true)
//            {
//                if (props.Parent == null)
//                {
//                    return props;
//                }
//                props = props.Parent;
//            }
//        }

//        private static string GetPropertyNameRoot(XmlSchemaElement props)
//        {
//            if (props.ElementSchemaType != null && !String.IsNullOrWhiteSpace(props.ElementSchemaType.Name))
//            {
//                return props.ElementSchemaType.Name.Replace(".", "");
//            }
//            if (props.SchemaTypeName != null && !String.IsNullOrWhiteSpace(props.SchemaTypeName.Name))
//            {
//                return props.SchemaTypeName.Name.Replace(".", "");
//            }
//            if (props.RefName != null && !String.IsNullOrWhiteSpace(props.RefName.Name))
//            {
//                return props.RefName.Name.Replace(".", "");
//            }
//            return "";
//        }



//        internal static void Compile(this WadlDto wadl)
//        {
//            var schemaSet = new XmlSchemaSet();
//            schemaSet.ValidationEventHandler += schemaSet_ValidationEventHandler;
//            schemaSet.CompilationSettings.EnableUpaCheck = false;
//            foreach (var xmlSchema in wadl.Wadl.Grammars.Include)
//            {
//                schemaSet.Add(xmlSchema.Xmlns, xmlSchema.Href);
//            }
//            schemaSet.Compile();
//            foreach (XmlSchema schema in schemaSet.Schemas())
//            {
//                wadl.Schema = schema;
//            }
//        }

//        private static void schemaSet_ValidationEventHandler(object sender, ValidationEventArgs e)
//        {
//            switch (e.Severity)
//            {
//                case XmlSeverityType.Warning:
//                    Console.Write("WARNING: ");
//                    break;
//                case XmlSeverityType.Error:
//                    Console.Write("ERROR: ");
//                    break;
//            }
//            Console.WriteLine(e.Message);
//        }
//    }
//}