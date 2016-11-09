using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Starterkit.Configuration.Web.Controllers.Api
{
    //public class MonitorController : ApiController
    //{
    //    private static ConcurrentDictionary<string,Dictionary<string,ConcurrentStack<MeasureValue>>> realtimeAnalysis=new ConcurrentDictionary<string, Dictionary<string, ConcurrentStack<MeasureValue>>>();

    //    [HttpPut]
    //    Task PutInvocationMeasurement([FromBody] MeasureValue item)
    //    {

    //        Dictionary<string, ConcurrentStack<MeasureValue>> setCollection;
    //        if (!realtimeAnalysis.TryGetValue(item.ConfigSetName, out setCollection))
    //        {
    //            setCollection=new Dictionary<string, ConcurrentStack<MeasureValue>>();
    //            realtimeAnalysis.TryAdd(item.ConfigSetName, setCollection);
    //        }
    //        ConcurrentStack<MeasureValue> measure;
    //        if (!setCollection.TryGetValue(item.ServiceHostName, out measure))
    //        {
    //            measure=new ConcurrentStack<MeasureValue>();
    //            measure.Push(item);
    //            setCollection.Add(item.ServiceHostName,measure);
    //        }
    //        MeasureValue value;
    //        if (measure.TryPeek(out value))
    //        {
    //            if(value.InternalSec==sec)
    //        }


    //    }
    //}

    public class MeasureValue
    {
        public string ServiceHostName { get; set; }

        public string EnvironmentName { get; set; }

        public string ConfigSetName { get; set; }
        
        public string serviceName { get; set; }
        
        internal long InvocationCount { get; set; }

    }
}
