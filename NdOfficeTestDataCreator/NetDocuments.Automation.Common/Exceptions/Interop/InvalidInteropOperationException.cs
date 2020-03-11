using System;

namespace NetDocuments.Automation.Common.Exceptions.Interop
{
    /// <summary>
    /// The exception which is thrown when an interop operation failed.
    /// </summary>
    public class InvalidInteropOperationException : Exception
    {
        public InvalidInteropOperationException()
        {
        }

        public InvalidInteropOperationException(string message)
            : base(message)
        {
        }

        public InvalidInteropOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
