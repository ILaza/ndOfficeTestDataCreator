using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    /// <summary>
    /// The exception which is thrown when item is not present on Folder view panel.
    /// </summary>
    public class ItemIsNotPresentException : Exception
    {
        public ItemIsNotPresentException()
        {
        }

        public ItemIsNotPresentException(string message)
            : base(message)
        {
        }

        public ItemIsNotPresentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
