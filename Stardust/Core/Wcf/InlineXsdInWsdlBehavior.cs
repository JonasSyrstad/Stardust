//
// InlineXsdInWsdlBehavior.cs
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

using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml.Schema;

namespace Stardust.Core.Wcf
{
    public class InlineXsdInWsdlBehavior
        : IWsdlExportExtension, IEndpointBehavior
    {

        #region IWsdlExportExtension Implementation
        //
        // IWsdlExportExtension Implementation
        //
        public void ExportContract(
            WsdlExporter exporter,
            WsdlContractConversionContext context
            )
        {
            // never called
        }

        public void ExportEndpoint(
            WsdlExporter exporter,
            WsdlEndpointConversionContext context
            )
        {
           
        }

        #endregion 


        #region Private Methods
        //
        // Private Methods
        //

        /// <summary>
        /// Recursively extract all the list of imported
        /// schemas
        /// </summary>
        /// <param name="schema">Schema to examine</param>
        /// <param name="schemaSet">SchemaSet with all referenced schemas</param>
        /// <param name="importsList">List to add imports to</param>
        private void AddImportedSchemas(
            XmlSchema schema,
            XmlSchemaSet schemaSet,
            ICollection<XmlSchema> importsList
            )
        {
            foreach (XmlSchemaImport import in schema.Includes)
            {
                var realSchemas =
                    schemaSet.Schemas(import.Namespace);
                foreach (XmlSchema ixsd in realSchemas)
                {
                    if (!importsList.Contains(ixsd))
                    {
                        importsList.Add(ixsd);
                        AddImportedSchemas(ixsd, schemaSet, importsList);
                    }
                }
            }
        }

        /// <summary>
        /// Remove any &lt;xsd:imports/&gt; in the schema
        /// </summary>
        /// <param name="schema">Schema to process</param>
        private void RemoveXsdImports(XmlSchema schema)
        {
            for (int i = 0; i < schema.Includes.Count; i++)
            {
                if (schema.Includes[i] is XmlSchemaImport)
                    schema.Includes.RemoveAt(i--);
            }
        }

        #endregion


        #region IEndpointBehavior Implementation
        //
        // IContractBehavior Implementation
        //
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters
            )
        {
            // not needed
        }

        public void ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime
            )
        {
            // not needed
        }

        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher dispatcher
            )
        {
            // not needed
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // not needed
        }

        #endregion 

    }
}