using System;

namespace NetDocuments.Automation.Common.Exceptions.Automation
{
    /// <summary>
    /// The exception which is thrown when web proxy is not operated properly.
    /// </summary>
    public class WebProxyException : Exception
    {
        public WebProxyException()
        {
        }

        public WebProxyException(string message)
            : base(message)
        {
        }

        public WebProxyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
