using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The exception which is thrown when 401 Unauthorized status code is returned from server.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
        {
        }

        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
