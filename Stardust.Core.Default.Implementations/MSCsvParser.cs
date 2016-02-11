using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Stardust.Nucleus;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;
using Stardust.Particles.TableParser;

namespace Stardust.Core.Default.Implementations
{
    public static class ParserExtensions
    {
        public static void OverrideCsvParser(this IConfigurator configurator)
        {
            configurator.UnBind<ITableParser>().FromAndRebind(Parsers.Delimitered).To<MSCsvParser>(Parsers.Delimitered).SetTransientScope().DisableOverride();
        }

        public static void OverrideCsvParser(this IConfigurator configurator,Action<string,Exception> errorHandler )
        {
            configurator.UnBind<ITableParser>().FromAndRebind(Parsers.Delimitered).To<MSCsvParser>(Parsers.Delimitered).SetTransientScope().DisableOverride();
            MSCsvParser.SetErrorHandler(errorHandler);
        }
    }

    class MSCsvParser : CsvTableParser, ITableParser
    {
        private static Action<string, Exception> parseErrorHandler;

        public override Document Parse(string fileName)
        {
            var parser = new TextFieldParser(fileName);
            return Parse(parser);
        }

        private Document Parse(TextFieldParser parser)
        {
            parser.Delimiters = new[] { Delimiter.ContainsCharacters() ? Delimiter : "|" };
            parser.TextFieldType=FieldType.Delimited;
            parser.CommentTokens=new[]{"#"};
            var doc=new Document();
            var isFirstLine = true;
            while (!parser.EndOfData)
            {
                string[] line;
                try
                {
                    line = parser.ReadFields();
                }
                catch (MalformedLineException mlex)
                {
                    Logging.DebugMessage("Unable to parse line {0}",mlex);
                    isFirstLine = false;
                    if (parseErrorHandler == null) throw;
                    parseErrorHandler(String.Format("Unable to parse line {0}",mlex.LineNumber), mlex);
                    continue;
                }
                catch (Exception ex)
                {
                    isFirstLine = false;
                    Logging.DebugMessage("Unable to parse line");
                    if (parseErrorHandler == null) throw;
                    parseErrorHandler("Unable to parse line", ex);
                    continue;
                }
                if(isFirstLine && IsFirstRowHeaders)
                    SetHeader(doc,CreateRow(line,doc));
                else
                {
                    AddRow(doc,CreateRow(line,doc));
                }
                isFirstLine = false;
            }
            return doc;
        }

        private DocumentRow CreateRow(string[] line, Document doc)
        {
            var row = (DocumentRow)ObjectFactory.Createinstance(typeof(DocumentRow),new object[]{doc});
            foreach (var value in line)
            {
                AddColumn(row,value);
            }
            return row;
        }

        public override Document Parse(Stream stream, bool buffered = false)
        {
            var parser = new TextFieldParser(stream);
            return Parse(parser);
        }

        public override Document Parse(byte[] buffer, bool buffered = false)
        {
            var parser = new TextFieldParser(new MemoryStream(buffer));
            return Parse(parser);
        }

        public static void SetErrorHandler(Action<string, Exception> errorHandler)
        {
            parseErrorHandler = errorHandler;
        }
    }
}
