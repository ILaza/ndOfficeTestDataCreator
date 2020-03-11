using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when Loading... indicator doesn't disappear after given time.
    /// </summary>
    public class LoadingTimeoutException : Exception
    {
        public LoadingTimeoutException()
        {
        }

        public LoadingTimeoutException(string message)
            : base(message)
        {
        }

        public LoadingTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
