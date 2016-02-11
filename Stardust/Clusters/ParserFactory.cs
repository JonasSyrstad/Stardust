//
// parserfactory.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Particles.TableParser;

namespace Stardust.Clusters
{
    public static class ParserFactory
    {
        private static Parsers DefaultParser = Parsers.Delimitered;

        public static void SetDefautParser(Parsers paser)
        {
            DefaultParser = paser;
        }

        public static ITableParser GetSuitableParser(string fileName)
        {
            try
            {
                return SearchForParser(fileName).First();
            }
            catch (Exception ex)
            {
                throw new StardustCoreException("No parser forund", ex);
            }
        }

        public static Document ParseFile(string fileName, Parsers parser, Func<ITableParser, ITableParser> initializer = null)
        {
            if (initializer.IsNull())
                return GetParser(parser).Parse(fileName);
            return initializer(GetParser(parser)).Parse(fileName);
        }

        [ExcludeFromCodeCoverage]
        public static Document ParseFile(string fileName, string parser, Func<ITableParser, ITableParser> initializer = null)
        {
            if (initializer.IsNull())
                return GetParser(parser).Parse(fileName);
            return initializer(GetParser(parser)).Parse(fileName);
        }

        public static ITableParser GetParser(this Parsers parser)
        {
            return Resolver.Activate<ITableParser>(parser);
        }

        [ExcludeFromCodeCoverage]
        public static ITableParser GetParser(string parser)
        {
            return Resolver.Activate<ITableParser>(parser);
        }

        [ExcludeFromCodeCoverage]
        public static ITableParser GetParser()
        {
            return DefaultParser.GetParser();
        }

        public static void SetParser<T>(Parsers parser) where T : ITableParser
        {
            SetParser<T>(parser.ToString());
        }

        public static void SetParser<T>(string parser) where T : ITableParser
        {
            Resolver.GetConfigurator().Bind<ITableParser>().To<T>(parser).SetTransientScope().DisableOverride();
        }

        [ExcludeFromCodeCoverage]
        public static void SetDefaultParser<T>() where T : ITableParser
        {
            Resolver.GetConfigurator().Bind<ITableParser>().To<T>().SetTransientScope().DisableOverride();
        }

        [ExcludeFromCodeCoverage]
        public static void SwitchXmlParserTo<T>() where T : ITableParser
        {
            SwitchParserTo<T>(Parsers.SimpleXmlParser);
        }

        [ExcludeFromCodeCoverage]
        public static void SwitchParserTo<T>(Parsers parser) where T : ITableParser
        {
            Resolver.GetConfigurator().UnBind<ITableParser>()
                .FromAndRebind(parser)
                .To<T>(parser);
        }

        [ExcludeFromCodeCoverage]
        public static IEnumerable<KeyValuePair<string, string>> GetAvailableImplementations()
        {
            return Resolver.GetImplementationsOf<ITableParser>();
        }

        private static IEnumerable<ITableParser> SearchForParser(string fileName)
        {
            return from parser in Resolver.ActivateAll<ITableParser>()
                   where parser.IsSuitableForFile(fileName)
                   select parser;
        }
    }
}