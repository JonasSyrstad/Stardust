using System;
using System.Diagnostics;
using System.Text;
using log4net;
using Stardust.Particles;

namespace Stardust.Core.CrossCuttingTest.LegacyTests.Mock
{
    public sealed class LoggingImplementation : ILogging
    {
        private string LogName = "StardustLogging";

        private ILog LogObject
        {
            get
            {
                return LogManager.GetLogger(LogName);
            }
        }

        private LoggingImplementation()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            LogException(exceptionToLog, additionalDebugInformation);
        }

        private void LogException(Exception exceptionToLog, string additionalDebugInformation)
        {
            var message = CreateDebugMessage(GetMessageAndStackTraceFromException(exceptionToLog), EventLogEntryType.Error, additionalDebugInformation);
            LogObject.Error(message);
            if (exceptionToLog.InnerException.IsInstance())
                LogException(exceptionToLog.InnerException, "Inner");
        }

        private static string GetMessageAndStackTraceFromException(Exception exceptionToLog)
        {
            var sb = new StringBuilder();
            sb.Append("Message: ");
            sb.Append(exceptionToLog.Message);
            sb.Append(" Stacktrace:\n");
            sb.Append(exceptionToLog.StackTrace);
            return sb.ToString();
        }

        public void HeartBeat()
        {
            DebugMessage("Heart Beat", EventLogEntryType.SuccessAudit, "Boom boom");
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information, string additionalDebugInformation = null)
        {
            var sb = CreateDebugMessage(message, entryType, additionalDebugInformation.ContainsCharacters() ? additionalDebugInformation : "");
            LogObject.Debug(sb.ToString());
        }

        private static StringBuilder CreateDebugMessage(string message, EventLogEntryType entryType, string additionalDebugInformation)
        {
            var sb = new StringBuilder();
            sb.Append(entryType.ToString());
            sb.Append(": ");
            sb.Append(message);
            if (additionalDebugInformation.ContainsCharacters())
            {
                sb.Append(" ---- Additional: ");
                sb.Append(additionalDebugInformation);
            }
            return sb;
        }

        public void SetCommonProperties(string logName)
        {
            if (logName.ContainsCharacters())
                LogName = logName;
        }
    }
}