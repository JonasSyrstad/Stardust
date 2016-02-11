//
// workflowfactory.cs
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
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xaml;
using Stardust.Particles;

namespace Stardust.Core.Workflow
{
    /// <summary>
    /// Helper class for loading and running XAML workflows
    /// </summary>
    /// <remarks>Remember to call WorkflowRunner.Using("assemblyname"); in order for WF to resolve your activies</remarks>
    public static class WorkflowFactory
    {
        internal static Assembly LocalAssembly;
        private const string InvalidXamlSource = "No xaml path or stream detected. Please provide a valid stream or path.";

        private static XamlXmlReaderSettings Settings
        {
            get { return new XamlXmlReaderSettings { LocalAssembly = LocalAssembly }; }
        }

        public static T CreateWorkflowContainer<T>(this byte[] xamlBuffer) where T : WorkflowContainerBase, new()
        {
            return CreateWorkflowContainer<T>(xamlBuffer.ToStream());
        }

        public static T CreateWorkflowContainer<T>() where T : WorkflowContainerBase, new()
        {
            return new T();
        }

        public static T CreateWorkflowContainer<T>(this Stream xamlStream) where T : WorkflowContainerBase, new()
        {
            return new T { XamlStream = xamlStream };
        }

        public static T RunWorkflow<T>() where T : WorkflowContainerBase, new()
        {
            var container = new T();
            return container.RunWorkflow();
        }

        public static T RunWorkflow<T>(this byte[] xamlBuffer) where T : WorkflowContainerBase, new()
        {
            var container = CreateWorkflowContainer<T>(xamlBuffer);
            return container.RunWorkflow();
        }

        public static T RunWorkflow<T>(Stream xamlStream) where T : WorkflowContainerBase, new()
        {
            var container = CreateWorkflowContainer<T>(xamlStream);
            return container.RunWorkflow();
        }

        public static T RunWorkflow<T>(this T container) where T : WorkflowContainerBase
        {
            AssertInArgumentsIsInstance(container);
            container.OutArguments = container.ExecuteWorkflow();
            return container;
        }

        public static IDictionary<string, object> RunXaml(string xamlPath, IDictionary<string, object> variables)
        {
            return xamlPath.Excecute(variables);
        }

        public static IDictionary<string, object> RunXaml(this Stream xamlStream, IDictionary<string, object> variables)
        {
            var xaml = new XamlXmlReader(xamlStream, Settings);
            return xaml.Excecute(variables);
        }

        public static IDictionary<string, object> RunXaml(this byte[] xamlBuffer, IDictionary<string, object> variables)
        {
            var xamlStream = xamlBuffer.ToStream();
            
            return xamlStream.RunXaml(variables);
        }

        /// <summary>
        /// Used to ensure that WF can resolve activity dependencies.
        /// </summary>
        /// <param name="assembly">the assembly name without .dll</param>
        public static void Using(string assembly)
        {
            assembly = assembly.ToLower().Replace(".dll", "");
            LocalAssembly = AppDomain.CurrentDomain.Load(assembly);
        }

        /// <summary>
        /// Register the assembly in workflow container
        /// </summary>
        /// <typeparam name="T">Any type in your entry assembly. Example a type in your web project or test project</typeparam>
        /// <remarks>This is added to ensure type safty and make the statement less prone to typos</remarks>
        public static void Using<T>()
        {
            Using(typeof(T).Assembly.FullName);
        }

        private static void AssertInArgumentsIsInstance<T>(T container) where T : WorkflowContainerBase
        {
            if (container.InArguments.IsNull())
                container.InArguments = new Dictionary<string, object>();
        }

        private static IDictionary<string, object> Excecute(this string xamlString, IDictionary<string, object> variables)
        {
            var activity = ActivityXamlServices.Load(xamlString, new ActivityXamlServicesSettings { CompileExpressions = true });
            return WorkflowInvoker.Invoke(activity, variables);
        }

        private static IDictionary<string, object> Excecute(this XamlReader reader, IDictionary<string, object> variables)
        {
            var activity = ActivityXamlServices.Load(reader, new ActivityXamlServicesSettings { CompileExpressions = true });
            return WorkflowInvoker.Invoke(activity, variables);
        }

        private static IDictionary<string, object> ExecuteWorkflow<T>(this T container) where T : WorkflowContainerBase
        {
            if (!container.Valid()) throw new StardustCoreException(String.Format("Container is invalid: {0}", container.ValidationMessage));
            if (container.XamlStream.IsInstance())
                return RunXaml(container.XamlStream, container.InArguments);
            if (container.XamlPath.IsNullOrWhiteSpace()) throw new StardustCoreException(InvalidXamlSource);
            return RunXaml(container.XamlPath, container.InArguments);
        }
    }
}