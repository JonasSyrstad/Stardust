using System.Collections.Generic;
using System.Linq;
using Stardust.Core.CrossCutting;

namespace Stardust.Core.Sandbox.Serializer
{
    public class FlatFileDeserializerImp : IFlatFileDeserializer
    {
        /// <summary>
        /// Deserializes the specified serialized objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObjects">The serialized objects.</param>
        /// <returns></returns>
        public T[] Deserialize<T>(string serializedObjects) where T : new()
        {
            var properties = typeof(T).GetProperties();
            var lines = serializedObjects.Split('\n');
            var headers = GetHeaders(lines[0].Replace("\r",""));
            var rows = GetRows(lines);
            var list = new List<T>();
            foreach (var row in rows)
            {
                if (row.Count() == 0) continue;
                var item = new T();
                var index = 0;
                foreach (var header in headers)
                {
                    var property = (from p in properties where p.Name == header select p).First();
                    property.SetValue(item, row.ElementAt(index));
                    index++;
                }
                list.Add(item);
            }
            return list.ToArray();
        }

        private IEnumerable<IEnumerable<string>> GetRows(string[] rows)
        {
            var isFirst = true;
            var convertedRows = new List<List<string>>();
            foreach (var row in rows)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                if (row.Replace("\r", "").IsNullOrWhiteSpace()) continue;
                convertedRows.Add(row.Replace("\r","").Split('|').ToList());
            }
            return convertedRows;
        }

        private IEnumerable<string> GetHeaders(string s)
        {
            return s.Split('|');
        }
    }
}