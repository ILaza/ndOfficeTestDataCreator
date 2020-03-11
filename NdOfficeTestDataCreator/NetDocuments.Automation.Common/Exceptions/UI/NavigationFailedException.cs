using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when automation framework has failed to navigate to the specified location.
    /// </summary>
    public class NavigationFailedException : Exception
    {
        public NavigationFailedException()
        {
        }

        public NavigationFailedException(string message)
            : base(message)
        {
        }

        public NavigationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
