using System;
using System.Collections.Generic;
using System.Globalization;

namespace Stardust.Core.Identity
{
    public interface IAttributeReader
    {
        KeyValuePair<string, string>[] GetAttributesForUser(string userIdentifier);
    }

    public class DemoAttributeReader : IAttributeReader
    {
        public KeyValuePair<string, string>[] GetAttributesForUser(string userIdentifier)
        {
            return new[]
                       {
                           new KeyValuePair<string, string>("http://test.no/author","Jonas"),
                           new KeyValuePair<string, string>("http://test.no/audience","DNV"),
                           new KeyValuePair<string, string>("http://test.no/runtime",DateTime.Now.ToShortDateString()),
                           new KeyValuePair<string, string>("http://test.no/appended",DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture))
                       };
        }
    }
}
