using System;

namespace Stardust.Rest.Client
{
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception innerException):base(message,innerException)
        {
        }
    }
}