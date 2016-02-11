using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Stardust.Core.CrossCutting;

namespace Stardust.Core.Sandbox.Serializer
{
    class FlatFileSerializerImp : IFlatFileSerializer
    {
        public void Serialize<T>(string filePath, T[] objectsToSerialize, Type type)
        {
            if (objectsToSerialize.IsNull()) throw new NullReferenceException("objects to serialize is null");
            var properties = type.GetProperties();
            var builder = BuildHader(properties);
            builder.AppendLine();
            AppendRows(objectsToSerialize, properties, builder);
            File.WriteAllText(filePath,builder.ToString());
        }

        private static void AppendRows<T>(IEnumerable<T> objectsToSerialize, PropertyInfo[] properties, StringBuilder builder)
        {
            foreach (var item in objectsToSerialize)
            {
                AppendRow(properties, builder, item);
            }
        }

        private static void AppendRow(IEnumerable<PropertyInfo> properties, StringBuilder builder, object item)
        {
            var isFirst = true;
            foreach (var propertyInfo in properties)
            {
                if (!isFirst)
                {
                    builder.Append("|");
                }
                builder.Append(propertyInfo.GetValue(item));
                isFirst = false;
            }
            builder.AppendLine();
        }

        private static StringBuilder BuildHader(IEnumerable<PropertyInfo> properties)
        {
            var builder = new StringBuilder();
            var isFirst = true;
            foreach (var propertyInfo in properties)
            {
                if (!isFirst)
                {
                    builder.Append("|");
                }
                builder.Append(propertyInfo.Name);
                isFirst = false;
            }
            return builder;
        }
    }
}