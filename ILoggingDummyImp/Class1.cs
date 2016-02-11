using System;
using System.Diagnostics;
using Stardust.Particles;

namespace ILoggingDummyImp
{
    public class Dummy : ILogging
    {
        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            //throw new InvalidOperationException();
        }

        public void HeartBeat()
        {
            throw new NotImplementedException();
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information, string additionalDebugInformation = null)
        {
            //throw new InvalidOperationException();
        }

        public void SetCommonProperties(string logName)
        {
            //throw new InvalidOperationException();
        }
    }
}
