//
// injectorbase.cs
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

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Nucleus.TypeResolver;
using Stardust.Particles;

namespace Stardust.Core.DynamicCompiler
{
    public abstract class InjectorBase : ICodeInjector
    {
        private const string CompilerVersion = "v4.0";
        protected CompilerParameters CompilerParams;
        protected IDictionary<string, string> ProviderOptions;

        protected InjectorBase()
        {
            CompilerParams = new CompilerParameters { GenerateExecutable = false, GenerateInMemory = true };
            CompilerParams.ReferencedAssemblies.Add("System.dll");
            ProviderOptions = new Dictionary<string, string>
                                  {  
                                      {"CompilerVersion",CompilerVersion}  
                                  };
        }

        public dynamic CompileAndExecute(string code, string method)
        {
            using (var provider = CreateProvider())
            {
                var result = Compile(code, provider);
                var instance = result.CompiledAssembly.CreateInstance(GetClassName(method));
                return ExecuteMember(method, instance);
            }
        }

        private CompilerResults Compile(string code, CodeDomProvider provider)
        {
            if (CompilatorOptimizer.HasResult(code.GetHashCode()))
                return CompilatorOptimizer.GetCompiledCode(code.GetHashCode());
            var result = provider.CompileAssemblyFromSource(CompilerParams, code);
            CompilatorOptimizer.StoreResult(code.GetHashCode(), result);
            return result;
        }

        protected abstract CodeDomProvider CreateProvider();

        private static dynamic ExecuteMember(string method, object instance)
        {
            var member = instance.GetType().GetMember(GetMethodName(method));
            return instance.GetType().InvokeMember(member.First().Name, InvokeAttributes, null, instance, null);
        }

        private static BindingFlags InvokeAttributes
        {
            get
            {
                return BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance;
            }
        }

        private static string GetMethodName(string method)
        {
            var parts = method.Split('.');
            return parts[parts.Length - 1];
        }

        private static string GetClassName(string method)
        {
            var parts = method.Split('.');
            var sb = new StringBuilder();
            for (int i = 0; i < parts.Length - 1; i++)
            {
                sb.Append(parts[i]);
                if (i < parts.Length - 2)
                    sb.Append(".");
            }
            return sb.ToString();
        }

        public T CompileAndCreateInstance<T>(string code)
        {
            using (var provider = CreateProvider())
            {
                var result = Compile(code, provider);
                var dynamicImplementation =TypeExtractor.RetreiveSubTypesFromAssembly(typeof(T), result.CompiledAssembly).FirstOrDefault();
                if (dynamicImplementation.IsNull()) throw new StardustCoreException("Not able to locate implementation");
                return ActivatorFactory.Activator.Activate<T>(dynamicImplementation);
            }
        }

        public ICodeInjector SetCompilerParameters(CompilerParameters parameters)
        {
            CompilerParams = parameters;
            return this;
        }

        public ICodeInjector SetProviderOptions(Dictionary<string, string> options)
        {
            ProviderOptions = options;
            return this;
        }

        public ICodeInjector AddReference(params string[] assemblies)
        {
            if (assemblies.IsNull()) return this;
            CompilerParams.ReferencedAssemblies.AddRange(assemblies);
            return this;
        }

        public ICodeInjector AddProviderOption(string key, string value)
        {
            ProviderOptions.Add(key, value);
            return this;
        }
    }
}