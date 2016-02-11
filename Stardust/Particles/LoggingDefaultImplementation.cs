//
// LoggingDefaultImplementation.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Stardust.Particles
{
    public sealed class LoggingDefaultImplementation : ILogging
    {
        private const string TabString = "      ";

        public void Exception(Exception exceptionToLog, string additionalDebugInformation = null)
        {
            var builder = new StringBuilder(string.Format("{1}{0}: ", DateTime.UtcNow, Environment.NewLine));
            builder.Append("Error: "+exceptionToLog.Message);
            if (additionalDebugInformation.ContainsCharacters())
                builder.AppendLine(string.Format("{0}{1}", TabString, additionalDebugInformation));
            builder.AppendLine(exceptionToLog.StackTrace.Replace(Environment.NewLine, string.Format("{0}{1}{1}", Environment.NewLine, TabString)));
            File.AppendAllText(GetFileLocation(), builder.ToString());
            if(exceptionToLog.InnerException.IsInstance())
                Exception(exceptionToLog.InnerException);
        }

        private string GetFileLocation()
        {
            var location = ConfigurationManagerHelper.GetValueOnKey("stardust.DefaultLoggerLocation");
            if (location.ContainsCharacters()) return location;
            return @"c:/temp/logging/defaultLog.txt";
        }

        public void HeartBeat()
        {
            File.AppendAllText(GetFileLocation(), string.Format("{0}: {1}", "Heartbeat", "Boom Boom"));
        }

        public void DebugMessage(string message, EventLogEntryType entryType = EventLogEntryType.Information, string additionalDebugInformation = null)
        {
            var builder = new StringBuilder(string.Format("{1}{0}: ", DateTime.UtcNow, Environment.NewLine));
            builder.AppendFormat("{0}: {1}", entryType, message);
            if (additionalDebugInformation.ContainsCharacters())
                builder.AppendLine(string.Format("{0}{1}", TabString, additionalDebugInformation));
            File.AppendAllText(GetFileLocation(), builder.ToString());
            
        }

        public void SetCommonProperties(string logName)
        {
        }
    }
}
