using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Stardust.Stellar
{
    [DataContract()]
    public class CallStackItem
    {
        public override string ToString()
        {
            return string.Join(".", Name, MethodName);
        }

        private DateTime? TaskEndTime;

        [DataMember]
        public string AdditionalInformation { get; set; }

        [DataMember]
        public List<CallStackItem> CallStack { get; set; }

        [DataMember]
        public DateTime? EndTimeStamp
        {
            get
            {
                return TaskEndTime;
            }
            set
            {
                if (value.HasValue)
                {
                    ExecutionTime = (value.Value - TimeStamp).TotalMilliseconds;
                }
                else
                    ExecutionTime = null;
                TaskEndTime = value;
            }
        }

        [DataMember]
        public string ErrorPath { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public double? ExecutionTime { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string StackTrace { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }
    }
}