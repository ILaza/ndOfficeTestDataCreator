using System;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// The class that holds methods to handle exceptions.
    /// </summary>
    public static class ExceptionManager
    {
        /// <summary>
        /// Executes the action and handles the exception if it occurs.
        /// </summary>
        /// <param name="action">The action that should be done in the try scope.</param>
        /// <param name="exceptionAction">The action that should be done in the catch scope.</param>
        /// <param name="finallyAction">The action that should be done in the finally scope.</param>
        /// <param name="throwException">True if it is necessary to throw exception, otherwise false.</param>
        public static void HandleIfException(Action action,
                                             Action<Exception> exceptionAction = null,
                                             Action finallyAction = null,
                                             bool throwException = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException("Action parameter is null");
            }

            try
            {
                action();
            }
            catch (Exception exception)
            {
                exceptionAction?.Invoke(exception);
                if (throwException)
                {
                    throw;
                }
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }
    }
}
