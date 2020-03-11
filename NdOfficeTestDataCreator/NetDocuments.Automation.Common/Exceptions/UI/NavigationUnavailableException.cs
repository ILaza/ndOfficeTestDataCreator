using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when navigation unavailable in Navigation panel.
    /// </summary>
    public class NavigationUnavailableException : Exception
    {
        public NavigationUnavailableException()
        {
        }

        public NavigationUnavailableException(string message)
            : base(message)
        {
        }

        public NavigationUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
