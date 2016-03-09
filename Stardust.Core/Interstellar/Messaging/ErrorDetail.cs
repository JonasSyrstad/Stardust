using System;
using System.Runtime.Serialization;

namespace Stardust.Interstellar.Messaging
{
    /// <summary>
    /// Contains information about the exception thrown at the server
    /// </summary>
    [DataContract]
    public class ErrorDetail
    {
        public static ErrorDetail GetDetails(Exception exception)
        {
            if (exception == null) return null;
            return new ErrorDetail
            {
                ExceptionMessage = exception.Message,
                ExceptionType = exception.GetType().FullName,
                InnerDetail = GetDetails(exception.InnerException)
            };
        }
        /// <summary>
        /// The inner exception if any
        /// </summary>
        [DataMember]
        public ErrorDetail InnerDetail { get; set; }

        /// <summary>
        /// The exception types full name
        /// </summary>
        [DataMember]
        public string ExceptionType { get; set; }

        /// <summary>
        /// the message of the exception
        /// </summary>
        [DataMember]
        public string ExceptionMessage { get; set; }
    }
}