//using System;
//using System.Text;
//using System.Xml.Schema;

//namespace Stardust.Stardust_Tooling.Wadl
//{
//    public static class WadlInterfaceBuilder
//    {
//        public static string GetType(this XmlSchema schema, string inputName)
//        {
//            var input = "void";
//            foreach (var item in schema.Items)
//            {
//                var elementType = item as XmlSchemaElement;
//                if (elementType != null)
//                {
//                    if (elementType.Name.EndsWith(inputName, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        input = elementType.Name;
//                    }
//                }
//            }
//            return input;
//        }

//        public static StringBuilder BuildInterface(this WadlDto wadl, StringBuilder builder)
//        {
//            builder.SetServiceContractAttribute(WadlExtensions.wcfNs)
//                .DeclareInterface(wadl.Wadl.Name.ToProperCase())
//                .BeginBlock(1);
//            var isFirst = true;
//            foreach (var resource in wadl.Wadl.Resources.Resource)
//            {
//                if (!isFirst) builder.AppendLine();
//                isFirst = false;
//                var type = wadl.GetSchemaType(resource);
//                var schema = wadl.Includes[type.ToLower()];
//                var input = schema.GetType("_input");
//                var output = GetType(schema, "_output");
//                var methodName = resource.Path.Replace("/", "").ToProperCase();
//                builder.AddOperationContractAttribute();
//                builder.DeclareInterfaceMethod(output == "void" ? "object" : output.ToProperCase(), methodName, input.ToProperCase());
//                builder.DeclareInterfaceMethodAsync(output == "void" ? "void" : output.ToProperCase(), methodName, input.ToProperCase());
//            }
//            builder.EndBlock(1);
//            return builder;
//        }

//        internal static string GetSchemaType(this WadlDto wadl, Resource resource)
//        {
//            var ns = resource.Method.Request.Representation[0].Type.Split(':')[0];
//            string type = null;
//            foreach (var xmlAttribute in wadl.Wadl.Attributes)
//            {
//                if (xmlAttribute.Name.EndsWith(ns))
//                {
//                    type = xmlAttribute.Value;
//                    break;
//                }
//            }
//            return type;
//        }
//    }
//}