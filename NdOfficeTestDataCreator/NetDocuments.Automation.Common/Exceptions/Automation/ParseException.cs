using System;

namespace NetDocuments.Automation.Common.Exceptions.Automation
{
    /// <summary>
    /// The exception which is thrown when parsing operation is failed.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
