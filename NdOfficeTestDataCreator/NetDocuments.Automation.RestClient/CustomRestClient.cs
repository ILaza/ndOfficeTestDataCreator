using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

using NetDocuments.Rest.Contracts.Enums;
using NetDocuments.Rest.Contracts.Enums.V2;
using NetDocuments.Rest.Contracts.Models;
using NetDocuments.Rest.Contracts.Models.V1;
using NetDocuments.Rest.Contracts.Models.V2;
using NetDocuments.Rest.Infrastructure.Interfaces;
using NetDocuments.Rest.Infrastructure.RequestParameters;
using NetDocuments.Rest.V1.Parameters;

namespace NetDocuments.Automation.RestClient
{
    /// <summary>
    /// Holds additional, overrided, etc REST API calls.
    /// </summary>
    public class CustomRestClient
    {
        private class ControllerMetadata : IControllerMetadata
        {
            /// <summary>
            /// Holds REST api version.
            /// </summary>
            public ApiVersion ApiVersion { get; set; }

            /// <summary>
            /// Holds base part of REST api url, e.g. 'repository', 'document', etc
            /// </summary>
            public string BaseUrlPart { get; set; }
        }

        private readonly IRequestBuilderFactory requestBuilderFactory;
        private readonly IClientProvider clientProvider;

        public CustomRestClient(IRequestBuilderFactory requestBuilderFactory, IClientProvider clientProvider)
        {
            this.requestBuilderFactory = requestBuilderFactory;
            this.clientProvider = clientProvider;
        }

        /// <summary>
        /// Gets repository groups ids.
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="startsWithName"></param>
        /// <param name="includeUnlisted"></param>
        /// <param name="top"></param>
        /// <param name="paging"></param>
        /// <param name="skipToken"></param>
        /// <param name="returnInfo"></param>
        /// <param name="token"></param>
        /// <returns><see cref="NdRepositoryGroupList"> instance.</returns>
        public OperationResult<NdRepositoryGroupList> GetRepositoryGroups(string repositoryId,
                                                                          string startsWithName = null,
                                                                          bool? includeUnlisted = null,
                                                                          int? top = null,
                                                                          bool? paging = null,
                                                                          string skipToken = null,
                                                                          bool returnInfo = false,
                                                                          CancellationToken token = default)
        {
            var builder = requestBuilderFactory
                .Get()
                .SetInitialData(new ControllerMetadata
                {
                    ApiVersion = ApiVersion.V1,
                    BaseUrlPart = "repository"
                })
                .SetMethod(HttpMethod.Get)
                .AddUriPart(repositoryId)
                .AddUriPart("groups");

            var filterParameter = new FilterParameter();

            if (!string.IsNullOrWhiteSpace(startsWithName))
            {
                filterParameter.AppendAndExpression(FilterParameter.StartsWithExpression, "name", startsWithName);
            }

            if (includeUnlisted.HasValue)
            {
                filterParameter.AppendAndExpression(FilterParameter.IncludeUnlistedExpression, includeUnlisted.Value.ToString().ToLower());
            }

            if (top.HasValue)
            {
                builder.AddQueryParameter(new TopParameter(Math.Min(10000, Math.Max(1, top.Value))));
            }

            var isPaginationUsed = false;

            if (!string.IsNullOrEmpty(skipToken))
            {
                isPaginationUsed = true;
                builder.AddQueryParameter(new SkipTokenParameter(skipToken));
            }

            if (paging.HasValue)
            {
                isPaginationUsed = paging.Value;
                builder.AddQueryParameter(new RequestParameter<bool>("paging", paging.Value));
            }

            if (returnInfo)
            {
                builder.AddQueryParameter(new RequestParameter<string>("returnInfo", "all"));
            }

            builder.AddQueryParameter(filterParameter);

            var request = builder.Build<NdRepositoryGroupList>();

            return isPaginationUsed
                ? request.Execute(clientProvider.GetDefaultClient(), token)
                : request.Execute<string[]>(clientProvider.GetDefaultClient(),
                                            (groups, cancellationToken) => new NdRepositoryGroupList { Items = groups },
                                            token);
        }

        /// <summary>
        /// Gets repository groups ids.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>A collection of <see cref="DocumentModel">.</returns>
        public OperationResult<List<DocumentModel>> GetDocumentList(IEnumerable<string> documentsId,
                                                                    IEnumerable<OptionalAttributes> optionalAttributes,
                                                                    CancellationToken token = default)
        {
            var builder = requestBuilderFactory
                .Get()
                .SetInitialData(new ControllerMetadata
                {
                    ApiVersion = ApiVersion.V2,
                    BaseUrlPart = "document"
                })
                .AddUriPart("list")
                .SetMethod(HttpMethod.Get);

            if (optionalAttributes.Any())
            {
                var a = string.Join(",", optionalAttributes);
                builder.AddQueryParameter(new RequestParameter<string>("select", a));
            }

            builder.AddQueryParameter(new RequestParameter<string>("id", string.Join(",", documentsId)));

            var request = builder.Build<List<DocumentModel>>();

            return request.Execute(clientProvider.GetDefaultClient(), token);
        }
    }
}
