using System;
using System.Threading;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Holds methods to realize wait strategy.
    /// </summary>
    public static class Wait
    {
        /// <summary>
        /// Waits until timeout exceeded for the given condition.
        /// </summary>
        /// <param name="condition">The condition to wait.</param>
        /// <param name="timeoutMilliSeconds">Max wait time. By default, 30 seconds.</param>
        /// <param name="exceptionMessage">
        /// Exception message if timeout exceeded.
        /// By default, "Timeout exceeded.".
        /// </param>
        /// <param name="retryRateDelayMilliSeconds">The retry delay value. By default, 50 milliseconds.</param>
        /// <exception cref="TimeoutException">Throws if timeout exceeded.</exception>
        public static void For(Func<bool> condition,
                               int timeoutMilliSeconds = 30000,
                               string exceptionMessage = "Timeout exceeded.",
                               int retryRateDelayMilliSeconds = 50)
        {
            var startedAt = DateTime.Now;
            while (!condition())
            {
                var runningTime = (DateTime.Now - startedAt).TotalMilliseconds;
                if (runningTime >= timeoutMilliSeconds)
                {
                    throw new TimeoutException(exceptionMessage);
                }
                Thread.Sleep(retryRateDelayMilliSeconds);
            }
        }

        /// <summary>
        /// Waits until timeout exceeded for the given condition. 
        /// </summary>
        /// <param name="condition">The condition to wait.</param>
        /// <param name="timeoutMilliSeconds">Max wait time. By default, 30 seconds.</param>
        /// <param name="retryRateDelayMilliSeconds">The retry delay value. By default, 50 milliseconds.</param>
        /// <returns>Returns true if action result true till wait time, otherwise false.</returns>
        public static bool ForResult(Func<bool> condition,
                                     int timeoutMilliSeconds = 30000,
                                     int retryRateDelayMilliSeconds = 50)
        {
            var startedAt = DateTime.Now;
            while (!condition())
            {
                var runningTime = (DateTime.Now - startedAt).TotalMilliseconds;
                if (runningTime >= timeoutMilliSeconds)
                {
                    return false;
                }
                Thread.Sleep(retryRateDelayMilliSeconds);
            }
            return true;
        }

        /// <summary>
        /// Waits until timeout exceeded for the given condition. 
        /// </summary>
        /// <param name="func">The function to wait for.</param>
        /// <param name="timeoutMilliSeconds">Max wait time. By default, 30 seconds.</param>
        /// <param name="retryRateDelayMilliSeconds">The retry delay value. By default, 50 milliseconds.</param>
        /// <returns>Returns action result or null.</returns>
        public static T ForResult<T>(Func<T> func,
                                     int timeoutMilliSeconds = 30000,
                                     int retryRateDelayMilliSeconds = 50) where T : class
        {
            var startedAt = DateTime.Now;
            T result = default(T);
            while ((result = func()) == null)
            {
                var runningTime = (DateTime.Now - startedAt).TotalMilliseconds;
                if (runningTime >= timeoutMilliSeconds)
                {
                    break;
                }
                Thread.Sleep(retryRateDelayMilliSeconds);
            }
            return result;
        }
    }
}
