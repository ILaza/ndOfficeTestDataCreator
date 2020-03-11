using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using NetDocuments.Rest;
using NetDocuments.Rest.Contracts.Controllers.V1;
using NetDocuments.Rest.Contracts.Controllers.V2;
using NetDocuments.Rest.Infrastructure;
using NetDocuments.Rest.Infrastructure.ContentFormatters;
using NetDocuments.Rest.Infrastructure.Helpers;
using NetDocuments.Rest.Infrastructure.Interfaces;
using NetDocuments.Rest.V1;
using NetDocuments.Rest.V1.Controllers;
using NetDocuments.Rest.V1.Controllers.Helpers;
using NetDocuments.Rest.V1.Parameters.Helpers;
using NetDocuments.Rest.V2;
using NetDocuments.Rest.V2.Controllers;

using DocumentV1 = NetDocuments.Rest.V1.Controllers.Document;
using SearchV1 = NetDocuments.Rest.V1.Controllers.Search;
using UserV1 = NetDocuments.Rest.V1.Controllers.User;
using CabinetV1 = NetDocuments.Rest.V1.Controllers.Cabinet;

using DocumentV2 = NetDocuments.Rest.V2.Controllers.Document;
using SearchV2 = NetDocuments.Rest.V2.Controllers.Search;
using UserV2 = NetDocuments.Rest.V2.Controllers.User;
using CabinetV2 = NetDocuments.Rest.V2.Controllers.Cabinet;

using NetDocuments.Automation.Common.Entities;
using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.RestClient.Infrastructure;

namespace NetDocuments.Automation.RestClient.Factories
{
    /// <summary>
    /// Factory class to create NetDocuments Rest Client facade.
    /// </summary>
    public class DefaultRestApiFactory : IRestApiFactory, IDisposable
    {
        private static readonly ConcurrentDictionary<string, Lazy<NdRestApiFacade>> restClients
            = new ConcurrentDictionary<string, Lazy<NdRestApiFacade>>();

        private bool disposedValue = false;

        public readonly HostSettings ndHostSettings;

        public DefaultRestApiFactory(HostSettings ndHostSettings)
        {
            this.ndHostSettings = ndHostSettings;
        }

        private static IV1Facade RestClientV1Initializer(IRequestBuilderFactory requestBuilderFactory,
                                                         IClientProvider clientProvider,
                                                         IContentFormattersFactory contentFormattersFactory)
        {
            var uploadProgressTracker = new UploadProgressTracker();
            var parameterHelper = new ParameterHelper();
            var executionHelper = new ExecutionHelper();

            var cabinet = new CabinetV1(requestBuilderFactory, clientProvider);
            var customAttributeValues = new CustomAttributeValues(requestBuilderFactory, clientProvider);

            var document = new DocumentV1(requestBuilderFactory,
                                          clientProvider,
                                          contentFormattersFactory,
                                          uploadProgressTracker);

            var filter = new Filter(requestBuilderFactory, clientProvider, parameterHelper, executionHelper);
            var folder = new Folder(requestBuilderFactory,
                                    clientProvider,
                                    parameterHelper,
                                    executionHelper,
                                    contentFormattersFactory);

            var group = new Group(clientProvider, requestBuilderFactory);
            var repository = new Repository(requestBuilderFactory, clientProvider);
            var savedSearch = new SavedSearch(requestBuilderFactory, clientProvider, parameterHelper, executionHelper);
            var search = new SearchV1(requestBuilderFactory, clientProvider, parameterHelper, executionHelper);
            var sync = new Sync(requestBuilderFactory, clientProvider);
            var user = new UserV1(requestBuilderFactory, clientProvider, parameterHelper, executionHelper);
            var workspace = new Workspace(requestBuilderFactory, clientProvider, parameterHelper, executionHelper);

            return new V1Facade(cabinet,
                                customAttributeValues,
                                document,
                                filter,
                                folder,
                                group,
                                repository,
                                savedSearch,
                                search,
                                sync,
                                user,
                                workspace);
        }

        private static IV2Facade RestClientV2Initializer(IRequestBuilderFactory requestBuilderFactory,
                                                         IClientProvider clientProvider,
                                                         IContentFormattersFactory contentFormattersFactory)
        {
            var container = new Container(requestBuilderFactory, clientProvider, contentFormattersFactory);
            var search = new SearchV2(requestBuilderFactory, clientProvider, contentFormattersFactory);
            var user = new UserV2(requestBuilderFactory, clientProvider);
            var document = new DocumentV2(requestBuilderFactory, clientProvider, contentFormattersFactory);
            var command = new Command(requestBuilderFactory, clientProvider, contentFormattersFactory);
            var cabinet = new CabinetV2(requestBuilderFactory, clientProvider);
            var service = new Service(requestBuilderFactory, clientProvider);

            return new V2Facade(container, search, user, document, command, cabinet, service);
        }

        /// <summary>
        /// Creates NdRestApiFacade instance.
        /// </summary>
        /// <param name="userInfo"><see cref="UserInfo"/> instance.</param>
        /// <returns>NdRestApiFacade instance.</returns>
        public NdRestApiFacade CreateApiClient(UserInfo userInfo)
        {
            var containerWrapper = new FormattersContainerWrapper();
            var contentFormattersFactory = new ContentFormattersFactory(containerWrapper);
            var authenticationStorageAdapter = new InMemoryAuthenticationStorageAdapter();
            var clientData = new ClientData(ndHostSettings, authenticationStorageAdapter);
            var requestBuilderFactory = new RequestBuilderFactory(contentFormattersFactory, clientData);

            var oauthLocker = new OAuthLocker();
            var ndThreadUrlFormatHelper = new NdThreadUrlFormatHelper();
            var oauthClientProvider = new OAuthClientProvider(clientData);

            var oAuth = new OAuth(requestBuilderFactory,
                                  oauthClientProvider,
                                  contentFormattersFactory);

            var oAuthFacade = new OAuthFacade(oAuth, clientData, userInfo);
            oAuthFacade.RefreshTokens();

            var clientProvider = new ClientProvider(oAuth, clientData, ndThreadUrlFormatHelper, oauthLocker);

            // Note: it seems that we are running our tests on a single host.
            return restClients.GetOrAdd(userInfo.UserName,
                                        new Lazy<NdRestApiFacade>(
                                            () => new NdRestApiFacade(RestClientV1Initializer(requestBuilderFactory,
                                                                                              clientProvider,
                                                                                              contentFormattersFactory),
                                                                      RestClientV2Initializer(requestBuilderFactory,
                                                                                              clientProvider,
                                                                                              contentFormattersFactory),
                                                                      new CustomRestClient(requestBuilderFactory, clientProvider),
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
