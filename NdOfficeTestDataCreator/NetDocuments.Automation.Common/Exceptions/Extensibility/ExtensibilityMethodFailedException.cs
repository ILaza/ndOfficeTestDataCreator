using System;

namespace NetDocuments.Automation.Common.Exceptions.Extensibility
{
    /// <summary>
    /// The exception which is thrown when the call of some Extensibility method is failed.
    /// </summary>
    public class ExtensibilityMethodFailedException : Exception
    {
        public ExtensibilityMethodFailedException()
        {
        }

        public ExtensibilityMethodFailedException(string message)
            : base(message)
        {
        }

        public ExtensibilityMethodFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
