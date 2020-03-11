using System;

namespace NetDocuments.Automation.Common.Exceptions
{
    /// <summary>
    /// The exception which is thrown when precondition is not fulfilled.
    /// </summary>
    public class PreconditionException : Exception
    {
        public PreconditionException()
        {
        }

        public PreconditionException(string message)
            : base(message)
        {
        }

        public PreconditionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
