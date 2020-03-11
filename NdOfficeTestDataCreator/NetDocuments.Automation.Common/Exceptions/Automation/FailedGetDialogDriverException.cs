using System;

namespace NetDocuments.Automation.Common.Exceptions.Automation
{
    /// <summary>
    /// The exception which is thrown when automation framework has failed to get dialog driver.
    /// </summary>
    public class FailedGetDialogDriverException : Exception
    {
        public FailedGetDialogDriverException()
        {
        }

        public FailedGetDialogDriverException(string message)
            : base(message)
        {
        }

        public FailedGetDialogDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
