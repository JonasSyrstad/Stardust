using System;
using System.Runtime.Serialization;

namespace Stardust.Interstellar.Messaging
{
    /// <summary>
    /// Represents an exception thrown within the service
    /// </summary>
    [DataContract]
    public class ErrorMessage
    {
        /// <summary>
        /// The runtime instance Id for tracing and debugging
        /// </summary>
        [DataMember]
        public Guid TicketNumber { get; set; }

        /// <summary>
        /// the topmost exception message
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="newTicket"></param>
        public ErrorMessage(Guid newTicket)
        {
            TicketNumber = newTicket;
        }

        /// <summary>
        /// </summary>
        public ErrorMessage()
        {
        }

        /// <summary>
        /// the direct call stack path to the faulting module.This requires that TracerFactory.StartTrace is executed
        /// </summary>
        [DataMember]
        public string FaultLocation { get; set; }

        /// <summary>
        /// Contains information about the exception thrown at the server
        /// </summary>
        [DataMember]
        public ErrorDetail Detail { get; set; }

    }
}