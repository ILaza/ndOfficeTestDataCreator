using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The exception which is thrown when 503 Service Unavailable status code is returned from server.
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException()
        {
        }

        public ServiceUnavailableException(string message)
            : base(message)
        {
        }

        public ServiceUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
