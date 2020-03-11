using NetDocuments.Automation.Common.Entities;

namespace NetDocuments.Automation.RestClient.Factories
{
    /// <summary>
    /// Interface for factories to create NetDocuments Rest Client facade.
    /// </summary>
    public interface IRestApiFactory
    {
        /// <summary>
        /// Creates NdRestApiFacade instance.
        /// </summary>
        /// <param name="userInfo"><see cref="UserInfo"/> instance.</param>
        /// <returns>NdRestApiFacade instance.</returns>
        NdRestApiFacade CreateApiClient(UserInfo userInfo);
    }
}
