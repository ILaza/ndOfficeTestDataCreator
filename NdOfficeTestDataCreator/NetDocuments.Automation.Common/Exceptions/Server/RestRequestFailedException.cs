using System;

namespace NetDocuments.Automation.Common.Exceptions.Server
{
    /// <summary>
    /// The generic exception which is thrown when REST call to ND Web is failed.
    /// </summary>
    public class RestRequestFailedException : Exception
    {
        public RestRequestFailedException()
        {
        }

        public RestRequestFailedException(string message)
            : base(message)
        {
        }

        public RestRequestFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}