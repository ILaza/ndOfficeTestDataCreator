using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when window is in incorrect state
    /// (e.g. size is too small and not UI elements are not visible).
    /// </summary>
    public class IncorrectWindowStateException : Exception
    {
        public IncorrectWindowStateException()
        {
        }

        public IncorrectWindowStateException(string message)
            : base(message)
        {
        }

        public IncorrectWindowStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
