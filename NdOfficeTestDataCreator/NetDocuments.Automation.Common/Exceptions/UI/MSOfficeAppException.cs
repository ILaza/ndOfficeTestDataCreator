using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when an error is occured with MS Office app.
    /// </summary>
    public class MSOfficeAppException : Exception
    {
        public MSOfficeAppException()
        {
        }

        public MSOfficeAppException(string message)
            : base(message)
        {
        }

        public MSOfficeAppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
