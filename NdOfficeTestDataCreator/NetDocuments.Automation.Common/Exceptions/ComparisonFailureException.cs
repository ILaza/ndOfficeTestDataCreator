using System;

namespace NetDocuments.Automation.Common.Exceptions
{
    /// <summary>
    /// The exception which is thrown when actual and expected result comparison is failed.
    /// </summary>
    public class ComparisonFailureException : Exception
    {
        public ComparisonFailureException()
        {
        }

        public ComparisonFailureException(string message)
            : base(message)
        {
        }

        public ComparisonFailureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
