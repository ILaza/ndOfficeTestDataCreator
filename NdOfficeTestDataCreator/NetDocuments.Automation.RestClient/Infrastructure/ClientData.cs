using System;
using System.Collections.Generic;

using NetDocuments.Client.Common.Contracts.Interfaces;
using NetDocuments.Rest.Contracts;
using NetDocuments.Rest.Contracts.Models.V1;
using NetDocuments.Rest.Infrastructure.Interfaces;
using NetDocuments.Rest.Infrastructure.Proxy;

using NetDocuments.Automation.Common.Settings;
using NetDocuments.Automation.Helpers;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// Holds methods to work with application related data.
    /// </summary>
    /// TODO: Think about how we can manage the lifecycle of tokens.
    public class ClientData : IClientData
    {
        private const string USER_AGENT_FORMAT = "{0} {1}";
        private const string APPLICATION_NAME = "ndOffice Automation";

        private readonly HostSettings hostSettings;
        private readonly IAuthenticationStorageAdapter authenticationStorageAdapter;

        /// <summary>
        /// <see cref="IClientData.HostName"/>
        /// </summary>
        public string HostName => hostSettings.HostName;

        /// <summary>
        /// <see cref="IClientData.ApiUri"/>
        /// </summary>
        public Uri ApiUri => new Uri(hostSettings.ApiUrl);

        /// <summary>
        /// <see cref="IClientData.IsRestTracingEnabled"/>
        /// </summary>
        public bool IsRestTracingEnabled { get; }

        /// <summary>
        /// <see cref="IClientData.IsContentServerEnabled"/>
        /// </summary>
        public bool IsContentServerEnabled { get; }

        /// <summary>
        /// <see cref="IClientData.ProxySettings"/>
        /// </summary>
        public ProxySettings ProxySettings { get; }

        /// <summary>
        /// Creates an instance of <see cref="ClientData"/>
        /// </summary>
        public ClientData(HostSettings hostSettings,
                          IAuthenticationStorageAdapter authenticationStorageAdapter)
        {
            this.hostSettings = hostSettings;
            this.authenticationStorageAdapter = authenticationStorageAdapter;

            ProxySettings = new ProxySettings
            {
                ProxyOption = ProxyOptions.Automatic
            };
        }

        /// <summary>
        /// <see cref="IClientData.WebResponseHandlers"/>
        /// </summary>
        public IEnumerable<IWebResponseHandler> WebResponseHandlers { get; set; }

        /// <summary>
        /// <see cref="IClientData.GetClientId"/>
        /// </summary>
        public string GetClientId()
        {
            return hostSettings.ClientId;
        }

        /// <summary>
        /// <see cref="IClientData.GetClientSecret(string)"/>
        /// </summary>
        public string GetClientSecret(string clientId)
        {
            return hostSettings.SecretId;
        }

        /// <summary>
        /// <see cref="IClientData.GetSavedAccessToken(string)"/>
        /// </summary>
        public string GetSavedAccessToken(string key)
        {
            return authenticationStorageAdapter.GetAccessToken(key);
        }

        /// <summary>
        /// <see cref="IClientData.GetSavedRefreshToken(string)"/>
        /// </summary>
        public string GetSavedRefreshToken(string key)
        {
            return authenticationStorageAdapter.GetRefreshToken(key);
        }

        /// <summary>
        /// <see cref="IClientData.GetUserAgentString"/>
        /// </summary>
        public string GetUserAgentString()
        {
            return string.Format(USER_AGENT_FORMAT,
                                 APPLICATION_NAME,
                                 WindowsSystemHelper.OsInfo);
        }

        /// <summary>
        /// <see cref="IClientData.HandleTokenExceptions(Exception, string)"/>
        /// </summary>
        public void HandleTokenExceptions(Exception exception, string clientId)
        {
            throw new Exception($"Token handling error {exception.Message}");
        }

        /// <summary>
        /// <see cref="IClientData.SaveTokens(NdRefreshTokenResponse, string)"/>
        /// </summary>
        public void SaveTokens(NdRefreshTokenResponse accessTokens, string key)
        {
            authenticationStorageAdapter.SaveTokens(accessTokens, key);
        }

        /// <summary>
        /// <see cref="IClientData.ShouldUpdateAccessToken(string)"/>
        /// </summary>
        public bool ShouldUpdateAccessToken(string key)
        {
            return authenticationStorageAdapter.GetAccessTokenExpirationDate(key) < DateTime.Now
                   && !string.IsNullOrWhiteSpace(authenticationStorageAdapter.RetrieveRefreshToken(key));
        }
    }
}
