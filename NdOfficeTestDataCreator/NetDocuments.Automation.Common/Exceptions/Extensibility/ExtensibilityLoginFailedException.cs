using System;

namespace NetDocuments.Automation.Common.Exceptions.Extensibility
{
    /// <summary>
    /// The exception which is thrown when login with Extensibility method is failed.
    /// </summary>
    public class ExtensibilityLoginFailedException : ExtensibilityMethodFailedException
    {
        public ExtensibilityLoginFailedException()
        {
        }

        public ExtensibilityLoginFailedException(string message)
            : base(message)
        {
        }

        public ExtensibilityLoginFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
