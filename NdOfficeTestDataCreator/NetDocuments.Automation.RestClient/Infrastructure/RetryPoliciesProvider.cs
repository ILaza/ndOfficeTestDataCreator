using System;
using System.Linq;
using System.Net;

using Polly;

using NetDocuments.Rest.Contracts.Models;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// Holds policies for REST retries
    /// </summary>
    public static class RetryPoliciesProvider
    {
        private const int MAX_RETRIES = 8;

        private static readonly HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        private static readonly HttpStatusCode[] httpStatusCodesWorthRetrying =
        {
            TooManyRequests,
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        };

        /// <summary>
        /// Gets retry policy.
        /// </summary>
        public static Policy<OperationResult> GetRetryPolicy()
        {
            return Policy.HandleResult<OperationResult>(r => httpStatusCodesWorthRetrying.Contains((HttpStatusCode)r.HttpStatusCode))
                         .WaitAndRetry(MAX_RETRIES, retry => TimeSpan.FromSeconds(1 << retry));
        }

        /// <summary>
        /// Gets retry policy.
        /// </summary>
        public static Policy<OperationResult<T>> GetRetryPolicy<T>()
        {
            return Policy.HandleResult<OperationResult<T>>(r => httpStatusCodesWorthRetrying.Contains((HttpStatusCode)r.HttpStatusCode))
                         .WaitAndRetry(MAX_RETRIES, retry => TimeSpan.FromSeconds(1 << retry));
        }
    }
}
