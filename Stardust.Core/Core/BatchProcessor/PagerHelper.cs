using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Interstellar.Trace;

namespace Stardust.Core.BatchProcessor
{
    public static class PagerHelper
    {
        public static int GetTotalPageCount<T>(this IEnumerable<T> cursor, int pageSize)
        {
            using (TracerFactory.StartTracer(cursor, "GetTotalPageCount"))
            {
                var itemCount = cursor.Count();
                return (int) Math.Ceiling((double) itemCount/pageSize);
            }
        }
    }
}