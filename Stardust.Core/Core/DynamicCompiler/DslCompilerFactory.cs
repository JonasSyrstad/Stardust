//
// DslCompilerFactory.cs
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

using Stardust.Nucleus;

namespace Stardust.Core.DynamicCompiler
{
    /// <summary>
    /// Remember the classes generated by your custom DSL compiler must implement IDslCode 
    /// </summary>
    public static class DslCompilerFactory
    {
        public static void BindCompiler<T>() where T : IDslCompiler
        {
            Resolver.GetConfigurator().Bind<IDslCompiler>().To<T>();
        }
        
        public static void BindCompiler<T>(string named) where T : IDslCompiler
        {
            Resolver.GetConfigurator().Bind<IDslCompiler>().To<T>(named);
        }

        public static IDslCompiler CreateCompiler(string named)
        {
            var compiler = Resolver.Activate<IDslCompiler>(named);
            return compiler;
        }

        public static IDslCompiler CreateCompiler()
        {
            var compiler = Resolver.Activate<IDslCompiler>();
            return compiler;
        }

        public static object CompileAndCreateInstance<T>(string dslName, string code) where T : IDslCode
        {
            var compiler = CreateCompiler(dslName);
            return CompileAndCreateInstance<T>(compiler, code);
        }

        public static object CompileAndCreateInstance<T>(string code) where T : IDslCode
        {
            var compiler = CreateCompiler();
            return CompileAndCreateInstance<T>(compiler, code);
        }

        private static object CompileAndCreateInstance<T>(IDslCompiler compiler, string code) where T : IDslCode
        {
            return CompilatorFactory.CreateCompiler(compiler.CompilesTo)
                .CompileAndCreateInstance<T>(compiler.CompileToDotNetLanguage(code));
        }
    }
}