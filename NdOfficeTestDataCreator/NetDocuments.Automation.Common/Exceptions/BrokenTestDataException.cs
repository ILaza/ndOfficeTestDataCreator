using System;

namespace NetDocuments.Automation.Common.Exceptions
{
    /// <summary>
    /// The exception which is thrown when test data is broken.
    /// </summary>
    public class BrokenTestDataException : Exception
    {
        public BrokenTestDataException()
        {
        }

        public BrokenTestDataException(string message)
            : base(message)
        {
        }

        public BrokenTestDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
