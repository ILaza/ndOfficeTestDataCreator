using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when some of the NetDocuments addins is disabled.
    /// </summary>
    public class AddinDisabledException : Exception
    {
        public AddinDisabledException()
        {
        }

        public AddinDisabledException(string message)
            : base(message)
        {
        }

        public AddinDisabledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
