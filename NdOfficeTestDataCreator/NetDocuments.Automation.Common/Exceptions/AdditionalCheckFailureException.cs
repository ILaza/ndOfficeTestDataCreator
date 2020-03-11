using System;

namespace NetDocuments.Automation.Common.Exceptions
{
    /// <summary>
    /// The exception which is thrown when additional check is failed.
    /// </summary>
    public class AdditionalCheckFailureException : Exception
    {
        public AdditionalCheckFailureException()
        {
        }

        public AdditionalCheckFailureException(string message)
            : base(message)
        {
        }

        public AdditionalCheckFailureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
