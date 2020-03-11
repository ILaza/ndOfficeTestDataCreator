using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when location is not present in Navigation tree.
    /// </summary>
    public class LocationIsNotPresentException : Exception
    {
        public LocationIsNotPresentException()
        {
        }

        public LocationIsNotPresentException(string message)
            : base(message)
        {
        }

        public LocationIsNotPresentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
