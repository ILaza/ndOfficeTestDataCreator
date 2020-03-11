using System;

namespace NetDocuments.Automation.Common.Exceptions.UI
{
    public class ElementNotFoundException : Exception
    {
        /// <summary>
        /// The exception which is thrown when some of the elements was not found on UI.
        /// </summary>
        public ElementNotFoundException()
        {
        }

        public ElementNotFoundException(string message)
            : base(message)
        {
        }

        public ElementNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
