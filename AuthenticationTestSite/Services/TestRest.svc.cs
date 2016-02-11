using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using Stardust.Interstellar;
using Stardust.Interstellar.DefaultImplementations;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace AuthenticationTestSite
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TestRest" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select TestRest.svc or TestRest.svc.cs at the Solution Explorer and start debugging.
    public class TestRest : DefaultServiceBase, ITestRest
    {
        public TestRest(IRuntime runtime)
            : base(runtime)
        {
        }

        public ComplexType DoWork(string id)
        {
            //throw new Exception("What happended?");
            return new ComplexType
            {
                Message = string.Format("Hi {0}", id),
                TimeStamp = DateTime.Now,
                UserName = Runtime.GetCurrentClaimsPrincipal().Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                IsDelegate = ((ClaimsIdentity)Runtime.GetCurrentClaimsPrincipal().Identity).Actor!=null,
                Actor = ((ClaimsIdentity)Runtime.GetCurrentClaimsPrincipal().Identity).Actor != null?((ClaimsIdentity)Runtime.GetCurrentClaimsPrincipal().Identity).Actor.Claims.First(c=>c.Type==ClaimTypes.NameIdentifier).Value:""
            };
        }
    }

    [DataContract]
    public class ComplexType
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool IsDelegate { get; set; }

        [DataMember]
        public string Actor { get; set; }
    }
}
