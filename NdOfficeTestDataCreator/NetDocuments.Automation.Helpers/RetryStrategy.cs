using System;
using System.Threading;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds methods to realize the retry strategy.
    /// </summary>
    public static class RetryStrategy
    {
        private const int TIMES_TO_RETRY = 3;

        /// <summary>
        /// Executes the passed method set amount of times.
        /// </summary>
        /// <typeparam name="T">The return type of the passed method.</typeparam>
        /// <param name="action">The action that should be executed set amount of times.</param>
        /// <param name="retryIntervalMilliSec">The retry interval.</param>
        /// <param name="retryCount">The amount of times to retry executing the method.</param>
        /// <returns>The result of the passed method.</returns>
        public static T Do<T>(Func<T> action,
                              int retryIntervalMilliSec = 50,
                              int retryCount = TIMES_TO_RETRY)
        {
            for (int retry = 0; retry < retryCount - 1; retry++)
            {
                try
                {
                    action();
                }
                catch
                {
                    // If the Exception is caught, we try again retryCount - 1 times.
                    // The last try returns result of action execution with exception if it exists.
                }
                finally
                {
                    Thread.Sleep(retryIntervalMilliSec);
                }
            }
            return action();
        }
    }
}
