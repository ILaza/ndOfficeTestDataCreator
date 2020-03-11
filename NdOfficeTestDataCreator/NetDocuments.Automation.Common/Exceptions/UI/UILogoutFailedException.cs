using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when UI logout procedure is failed.
    /// </summary>
    public class UILogoutFailedException : Exception
    {
        public UILogoutFailedException()
        {
        }

        public UILogoutFailedException(string message)
            : base(message)
        {
        }

        public UILogoutFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
