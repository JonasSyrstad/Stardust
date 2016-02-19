using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Stardust.Frontier.BatchProcessor
{
    [DataContract]
    public class PagedList<T>
    {
        [DataMember]
        public IEnumerable<T> Items { get; set; }

        [DataMember]
        public int Page { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int TotalPages { get; set; }
    }
}