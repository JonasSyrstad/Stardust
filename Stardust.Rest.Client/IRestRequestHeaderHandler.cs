using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Rest.Client
{
    public interface IRestRequestHeaderHandler:IRestHeaderHandlerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">a serializable object to put in the request header</typeparam>
        /// <param name="headerHandler"></param>
        void InsertHeader<T>(Func<T> headerHandler );
    }

    public interface IRestResponseHeaderHandler:IRestHeaderHandlerBase
    {
        
    }

    public interface IRestHeaderHandlerBase
    {
        string HeaderName { get; }
    }
}
