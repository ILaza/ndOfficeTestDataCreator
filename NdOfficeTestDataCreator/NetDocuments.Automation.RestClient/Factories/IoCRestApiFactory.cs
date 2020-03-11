using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Autofac;
using NetDocuments.Rest.Contracts.Controllers.V1;
using NetDocuments.Rest.Contracts.Controllers.V2;
using NetDocuments.Rest.Contracts.Controllers;
using NetDocuments.Rest.Infrastructure.Interfaces;

using NetDocuments.Automation.Common.Entities;
using NetDocuments.Automation.RestClient.Infrastructure;

namespace NetDocuments.Automation.RestClient.Factories
{
    /// <summary>
    /// Factory class to create NetDocuments Rest Client facade.
    /// </summary>
    public class IoCRestApiFactory : IRestApiFactory, IDisposable
    {
        private static readonly ConcurrentDictionary<string, Lazy<NdRestApiFacade>> restClients
            = new ConcurrentDictionary<string, Lazy<NdRestApiFacade>>();

        private bool disposedValue = false;

        public readonly IComponentContext context;

        public IoCRestApiFactory(IComponentContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates NdRestApiFacade instance.
        /// </summary>
        /// <param name="userInfo"><see cref="UserInfo"/> instance.</param>
        /// <returns>NdRestApiFacade instance.</returns>
        public NdRestApiFacade CreateApiClient(UserInfo userInfo)
        {
            var oAuthFacade = new OAuthFacade(context.Resolve<IOAuth>(),
                                              context.Resolve<IClientData>(),
                                              userInfo);
            oAuthFacade.RefreshTokens();

            // Note: it seems that we are running our tests on a single host.
            return restClients.GetOrAdd(userInfo.UserName,
                                        new Lazy<NdRestApiFacade>(
                                            () => new NdRestApiFacade(context.Resolve<IV1Facade>(),
                                                                      context.Resolve<IV2Facade>(),
                                                                      context.Resolve<CustomRestClient>(),
                                                                      oAuthFacade)))
                              .Value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Parallel.ForEach(restClients, c => c.Value.Value.Dispose());
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
