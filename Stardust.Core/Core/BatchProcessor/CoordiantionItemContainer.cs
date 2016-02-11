using System;
using System.Collections.Generic;

namespace Stardust.Core.BatchProcessor
{
    public class CoordiantionItemContainer : ICoordinatedItemContainer
    {
        public object MasterItem { get; set; }
        public Dictionary<string, object> Sources { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}