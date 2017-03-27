using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Stardust.Accretion;

namespace Stardust.Stardust_Tooling
{
    [Guid("67C43CA0-F045-402A-8241-2B107BCFDC87")]
    public class ConfigGenerator : IVsSingleFileGenerator
    {
        /// <summary>
        /// Retrieves the file extension that is given to the output file name.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pbstrDefaultExtension">[out, retval] Returns the file extension that is to be given to the output file name. The returned extension must include a leading period. </param>
        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".stardust.cs";
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        private bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None || certificate.Subject.Contains("terstest1-vm1.cloudapp.net")|| certificate.Subject.Contains("*.azurewebsites.net");
        }

        /// <summary>
        /// Executes the transformation and returns the newly generated output file, whenever a custom tool is loaded, or the input file is saved.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="wszInputFilePath">[in] The full path of the input file. May be null in future releases of Visual Studio, so generators should not rely on this value.</param><param name="bstrInputFileContents">[in] The contents of the input file. This is either a UNICODE BSTR (if the input file is text) or a binary BSTR (if the input file is binary). If the input file is a text file, the project system automatically converts the BSTR to UNICODE.</param><param name="wszDefaultNamespace">[in] This parameter is meaningful only for custom tools that generate code. It represents the namespace into which the generated code will be placed. If the parameter is not null and not empty, the custom tool can use the following syntax to enclose the generated code.   ' Visual Basic Namespace [default namespace]... End Namespace// Visual C#namespace [default namespace] { ... }</param><param name="rgbOutputFileContents">[out] Returns an array of bytes to be written to the generated file. You must include UNICODE or UTF-8 signature bytes in the returned byte array, as this is a raw stream. The memory for <paramref name="rgbOutputFileContents"/> must be allocated using the .NET Framework call, System.Runtime.InteropServices.AllocCoTaskMem, or the equivalent Win32 system call, CoTaskMemAlloc. The project system is responsible for freeing this memory.</param><param name="pcbOutput">[out] Returns the count of bytes in the <paramref name="rgbOutputFileContent"/> array.</param><param name="pGenerateProgress">[in] A reference to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsGeneratorProgress"/> interface through which the generator can report its progress to the project system.</param>
        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            var generator = new Generator();
            ICodeFileWriterContext generatedFile;
            if (bstrInputFileContents.StartsWith("{"))
            {
                generatedFile = generator.CreateFile(wszDefaultNamespace, bstrInputFileContents);
            }
            else
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = CertificateValidation;
                    var reader = new WebClient();

                    reader.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["stardust.configUser"], ConfigurationManager.AppSettings["stardust.configPassword"], ConfigurationManager.AppSettings["stardust.configDomain"]);
                    var content = reader.DownloadString(string.Format("{0}&updV={1}", bstrInputFileContents, DateTime.Now.Ticks));
                    generatedFile = generator.CreateFile(wszDefaultNamespace, content);
                }
                catch (Exception ex)
                {
                    
                    throw new Exception("Unable to download settings from: "+bstrInputFileContents);
                }
            }
            pcbOutput = (uint)generator.Save(generatedFile, rgbOutputFileContents);
            return VSConstants.S_OK;
        }
    }
}
