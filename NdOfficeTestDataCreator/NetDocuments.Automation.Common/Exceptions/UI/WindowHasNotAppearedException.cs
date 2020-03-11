using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when a particular window has not appeared on the screen.
    /// </summary>
    public class WindowHasNotAppearedException : Exception
    {
        public WindowHasNotAppearedException()
        {
        }

        public WindowHasNotAppearedException(string message)
            : base(message)
        {
        }

        public WindowHasNotAppearedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
