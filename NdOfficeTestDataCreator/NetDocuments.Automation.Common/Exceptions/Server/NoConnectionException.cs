using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The exception which is thrown when connection to the server is unavailable.
    /// </summary>
    public class NoConnectionException : Exception
    {
        public NoConnectionException()
        {
        }

        public NoConnectionException(string message)
            : base(message)
        {
        }

        public NoConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
