using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The exception which is thrown when 404 Not Found status code is returned from server.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
