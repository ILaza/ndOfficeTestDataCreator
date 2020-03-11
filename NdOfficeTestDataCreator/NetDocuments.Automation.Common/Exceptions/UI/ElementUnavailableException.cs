using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when some of the elements is not available on UI.
    /// </summary>
    public class ElementUnavailableException : Exception
    {
        public ElementUnavailableException()
        {
        }

        public ElementUnavailableException(string message)
            : base(message)
        {
        }

        public ElementUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
