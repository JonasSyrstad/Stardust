using System.Globalization;
using System.Linq;
using System.Text;

namespace Stardust.Accretion
{
    public static class WriterFactory
    {
        public static ICodeFileWriterContext CreateWriter()
        {
            return new WriterContext();
        }

        internal static StringBuilder AppendIndentedLine(this StringBuilder builder, int indentation, string line)
        {
            for (int i = 0; i < indentation; i++)
            {
                builder.Append("\t");
            }
            builder.AppendLine(line);
            return builder;
        }

        public static string GetValueOnKey(this ICodeWriterContext context, string key)
        {
            return string.Format("ConfigurationManagerHelper.GetValueOnKey(\"{0}\")", key);
        }

        
        
        public static string ToProperCase(this string text)
        {
            var myTi = new CultureInfo("en-US", false).TextInfo;
            return myTi.ToTitleCase(text.ToLower());
        }
    }
}