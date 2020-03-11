using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when UI login procedure is failed.
    /// </summary>
    public class UILoginFailedException : Exception
    {
        public UILoginFailedException()
        {
        }

        public UILoginFailedException(string message)
            : base(message)
        {
        }

        public UILoginFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
