using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The exception which is thrown when 403 Forbidden status code is returned from server.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
