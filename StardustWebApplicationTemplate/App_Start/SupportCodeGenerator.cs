using System;
using Stardust.Core.Service.Web;

namespace StardustWebApplicationTemplate
{
    public class SupportCodeGenerator : ISupportCodeGenerator
    {
        public string CreateSupportCode()
        {
            //Replace with your custom code generator
            return Guid.NewGuid().ToString();
        }
    }
}