using Autofac;
using NetDocuments.Client.Common.Contracts.Interfaces;
using NetDocuments.IoC.Interfaces;
using NetDocuments.Rest;
using NetDocuments.Rest.Contracts.Controllers;
using NetDocuments.Rest.Contracts.Controllers.V1;
using NetDocuments.Rest.Contracts.Controllers.V2;
using NetDocuments.Rest.Infrastructure;
using NetDocuments.Rest.Infrastructure.ContentFormatters;
using NetDocuments.Rest.Infrastructure.Helpers;
using NetDocuments.Rest.Infrastructure.Helpers.Interfaces;
using NetDocuments.Rest.Infrastructure.Interfaces;
using NetDocuments.Rest.V1;
using NetDocuments.Rest.V1.Controllers;
using NetDocuments.Rest.V1.Controllers.Helpers;
using NetDocuments.Rest.V1.Parameters.Helpers;
using NetDocuments.Rest.V2;
using NetDocuments.Rest.V2.Controllers;

using ICabinetV1 = NetDocuments.Rest.Contracts.Controllers.V1.ICabinet;
using IDocumentV1 = NetDocuments.Rest.Contracts.Controllers.V1.IDocument;
using ISearchV1 = NetDocuments.Rest.Contracts.Controllers.V1.ISearch;
using IUserV1 = NetDocuments.Rest.Contracts.Controllers.V1.IUser;
using ICabinetV2 = NetDocuments.Rest.Contracts.Controllers.V2.ICabinet;
using IContainerV2 = NetDocuments.Rest.Contracts.Controllers.V2.IContainer;
using IDocumentV2 = NetDocuments.Rest.Contracts.Controllers.V2.IDocument;
using ISearchV2 = NetDocuments.Rest.Contracts.Controllers.V2.ISearch;
using IUserV2 = NetDocuments.Rest.Contracts.Controllers.V2.IUser;
using CabinetV1 = NetDocuments.Rest.V1.Controllers.Cabinet;
using DocumentV1 = NetDocuments.Rest.V1.Controllers.Document;
using SearchV1 = NetDocuments.Rest.V1.Controllers.Search;
using UserV1 = NetDocuments.Rest.V1.Controllers.User;
using CabinetV2 = NetDocuments.Rest.V2.Controllers.Cabinet;
using DocumentV2 = NetDocuments.Rest.V2.Controllers.Document;
using SearchV2 = NetDocuments.Rest.V2.Controllers.Search;
using UserV2 = NetDocuments.Rest.V2.Controllers.User;

using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.RestClient.Infrastructure;

namespace NetDocuments.Automation.RestClient.IoC
{
    public class RestClientComponentsModule : Module
    {
        public HostSettings HostSettings { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FormattersContainerWrapper>()
                   .As<IInjectionContainer>()
                   .SingleInstance();

            builder.RegisterType<ContentFormattersFactory>()
                   .As<IContentFormattersFactory>()
                   .SingleInstance();

            builder.RegisterType<RequestBuilderFactory>()
                   .As<IRequestBuilderFactory>()
                   .SingleInstance();

            builder.RegisterType<OAuthLocker>()
                   .As<IOAuthLocker>()
                   .SingleInstance();

            builder.RegisterType<NdThreadUrlFormatHelper>()
                   .As<INdThreadUrlFormatHelper>()
                   .SingleInstance();

            builder.RegisterType<OAuthClientProvider>()
                   .As<IOAuthClientProvider>()
                   .SingleInstance();

            builder.RegisterType<OAuth>()
                   .As<IOAuth>()
                   .SingleInstance();

            builder.RegisterType<InMemoryAuthenticationStorageAdapter>()
                   .As<IAuthenticationStorageAdapter>()
                   .SingleInstance();

            builder.Register(ctx => new ClientData(HostSettings, ctx.Resolve<IAuthenticationStorageAdapter>()))
                   .As<IClientData>()
                   .SingleInstance();

            builder.RegisterType<ClientProvider>()
                   .As<IClientProvider>()
                   .SingleInstance();

            builder.RegisterType<UploadProgressTracker>()
                   .As<IProgressTracker>()
                   .SingleInstance();

            builder.RegisterType<ParameterHelper>()
                   .As<IParameterHelper>()
                   .SingleInstance();

            builder.RegisterType<ExecutionHelper>()
                   .As<IExecutionHelper>()
                   .SingleInstance();

            builder.RegisterType<CabinetV1>()
                   .As<ICabinetV1>()
                   .SingleInstance();

            builder.RegisterType<CustomAttributeValues>()
                   .As<ICustomAttributeValues>()
                   .SingleInstance();

            builder.RegisterType<DocumentV1>()
                   .As<IDocumentV1>()
                   .SingleInstance();

            builder.RegisterType<Filter>()
                   .As<IFilter>()
                   .SingleInstance();

            builder.RegisterType<Folder>()
                   .As<IFolder>()
                   .SingleInstance();

            builder.RegisterType<Group>()
                   .As<IGroup>()
                   .SingleInstance();
            
            builder.RegisterType<Repository>()
                   .As<IRepository>()
                   .SingleInstance();

            builder.RegisterType<SavedSearch>()
                   .As<ISavedSearch>()
                   .SingleInstance();

            builder.RegisterType<SearchV1>()
                   .As<ISearchV1>()
                   .SingleInstance();

            builder.RegisterType<Sync>()
                   .As<ISync>()
                   .SingleInstance();

            builder.RegisterType<UserV1>()
                   .As<IUserV1>()
                   .SingleInstance();

            builder.RegisterType<Workspace>()
                   .As<IWorkspace>()
                   .SingleInstance();

            builder.RegisterType<V1Facade>()
                   .As<IV1Facade>()
                   .SingleInstance();

            builder.RegisterType<Container>()
                   .As<IContainerV2>()
                   .SingleInstance();

            builder.RegisterType<SearchV2>()
                   .As<ISearchV2>()
                   .SingleInstance();

            builder.RegisterType<UserV2>()
                   .As<IUserV2>()
                   .SingleInstance();

            builder.RegisterType<DocumentV2>()
                   .As<IDocumentV2>()
                   .SingleInstance();

            builder.RegisterType<Command>()
                   .As<ICommand>()
                   .SingleInstance();

            builder.RegisterType<CabinetV2>()
                   .As<ICabinetV2>()
                   .SingleInstance();

            builder.RegisterType<Service>()
                   .As<IService>()
                   .SingleInstance();

            builder.RegisterType<V2Facade>()
                   .As<IV2Facade>()
                   .SingleInstance();

            builder.RegisterType<CustomRestClient>()
                   .AsSelf()
                   .SingleInstance();
        }

    }
}
