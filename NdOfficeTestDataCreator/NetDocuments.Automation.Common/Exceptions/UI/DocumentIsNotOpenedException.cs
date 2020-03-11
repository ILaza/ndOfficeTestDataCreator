using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    public class DocumentIsNotOpenedException : Exception
    {
        /// <summary>
        /// The exception which is thrown when some document was not opened on MS Office application.
        /// </summary>
        public DocumentIsNotOpenedException()
        {
        }

        public DocumentIsNotOpenedException(string message)
            : base(message)
        {
        }

        public DocumentIsNotOpenedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
