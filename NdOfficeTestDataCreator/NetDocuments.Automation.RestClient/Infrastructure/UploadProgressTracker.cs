using System;
using System.Collections.Generic;
using System.Net.Http;

using NetDocuments.Rest.Infrastructure.Interfaces;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// IProgressTracker implementation.
    /// For ndOffice REST client registration needs.
    /// </summary>
    public class UploadProgressTracker : IProgressTracker
    {
        /// <summary>
        /// <see cref="IProgressTracker.DisposeHandlers(IEnumerable{DelegatingHandler})"/>
        /// </summary>
        public void DisposeHandlers(IEnumerable<DelegatingHandler> progressHandlers)
        {
        }

        /// <summary>
        /// <see cref="IProgressTracker.GetHandlers(string, int)"/>
        /// </summary>
        public DelegatingHandler[] GetHandlers(string envelopeId, int versionNumber)
        {
            return Array.Empty<DelegatingHandler>();
        }
    }
}
