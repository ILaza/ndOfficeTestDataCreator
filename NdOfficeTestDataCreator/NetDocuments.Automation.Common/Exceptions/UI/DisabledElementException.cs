using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when some of the UI elements is disabled.
    /// </summary>
    public class DisabledElementException : Exception
    {
        public DisabledElementException()
        {
        }

        public DisabledElementException(string message)
            : base(message)
        {
        }

        public DisabledElementException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
