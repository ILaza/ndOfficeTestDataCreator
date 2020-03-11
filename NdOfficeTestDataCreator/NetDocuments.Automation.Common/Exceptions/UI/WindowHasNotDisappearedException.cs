using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when a particular window has not disappeared from the screen.
    /// </summary>
    public class WindowHasNotDisappearedException : Exception
    {
        public WindowHasNotDisappearedException()
        {
        }

        public WindowHasNotDisappearedException(string message)
            : base(message)
        {
        }

        public WindowHasNotDisappearedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
