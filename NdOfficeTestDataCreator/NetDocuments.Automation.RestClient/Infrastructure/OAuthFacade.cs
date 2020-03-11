using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NetDocuments.Rest.Contracts.Controllers;
using NetDocuments.Rest.Contracts.Enums;
using NetDocuments.Rest.Contracts.Models;
using NetDocuments.Rest.Contracts.Models.V1;
using NetDocuments.Rest.Infrastructure.Interfaces;
using NetDocuments.Rest.Infrastructure.Proxy;

using NetDocuments.Automation.Common.Entities;
using NetDocuments.Automation.RestClient.Models;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// Holds functionality for obtaining authorization code from REST.
    /// </summary>
    public class OAuthFacade
    {
        private const string REQUEST_LOGIN_FORM_FORMAT = "https://{0}/neWeb2/mobile/login.aspx";
        private const string REQUEST_OAUTH_AUTHORIZATION_URL_FORMAT = @"https://{0}/neWeb2/OAuth.aspx?client_id={1}&scope=full&response_type=code&redirect_uri={2}";
        private const string AUTHORIZE_CLIENT_REQUEST_FORMAT = "https://{0}/neWeb2/ajax/OAuthAjax.aspx?xt={1}&client_id={2}&scope=full&acceptType=JSON&command=authorizeClient";

        private const string VALID_REDIRECT_URI = "https://localhost/";
        private const string COOKIE_HEADER_NAME = "Cookie";
        private const string JSON_MEDIATYPE = "application/json";

        private static readonly Regex xtCodeRegex = new Regex("\"xt\": \"(?<xtCode>\\w+)\"", RegexOptions.Compiled);

        private readonly IOAuth oAuth;
        private readonly IClientData clientData;
        private readonly UserInfo userInfo;

        /// <summary>
        /// Creates a new instance of the OAuthFacade class.
        /// </summary>
        /// <param name="oAuth"><see cref="IOAuth"/> instance.</param>
        /// <param name="clientData"><see cref="IClientData"/> instance.</param>
        /// <param name="userInfo">User's credentials.</param>
        public OAuthFacade(IOAuth oAuth,
                           IClientData clientData,
                           UserInfo userInfo)
        {
            this.oAuth = oAuth;
            this.clientData = clientData;
            this.userInfo = userInfo;
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        /// <summary>
        /// Obtains an access and refresh tokens by authorization code.
        /// </summary>
        /// <returns><see cref=NdRefreshTokenResponse> instance.</returns>
        public NdRefreshTokenResponse ObtainTokensByCode()
        {
            var code = GetAuthorizationCodeAsync().ConfigureAwait(false)
                                                  .GetAwaiter()
                                                  .GetResult();

            var tokens = oAuth.ObtainTokensByCode(code,
                                                  VALID_REDIRECT_URI,
                                                  clientData.GetClientId(),
                                                  clientData.GetClientSecret(clientData.GetClientId()))
                              .ToResultOrException();

            clientData.SaveTokens(tokens, clientData.GetClientId());

            return tokens;
        }

        /// <summary>
        /// Obtains an access and refresh tokens by refresh token
        /// </summary>
        /// <returns><see cref=NdRefreshTokenResponse> instance.</returns>
        public NdRefreshTokenResponse ObtainTokensByRefreshToken()
        {
            var refreshToken = clientData.GetSavedRefreshToken(clientData.GetClientId());
            var tokens = oAuth.ObtainTokensByRefreshToken(refreshToken,
                                                          clientData.GetClientId(),
                                                          clientData.GetClientSecret(clientData.GetClientId()))
                              .ToResultOrException();

            clientData.SaveTokens(tokens, clientData.GetClientId());

            return tokens;
        }

        /// <summary>
        /// Revokes refresh token.
        /// </summary>
        /// <param name="host">Host which will be used to create a rest client.</param>
        /// <returns>True if revoke operation succedes; otherwise false.</returns>
        public NdRevokeTokenResponse RevokeRefreshToken(string host = null)
        {
            var refreshToken = clientData.GetSavedRefreshToken(clientData.GetClientId());
            return RevokeToken(refreshToken, TokenTypeHint.RefreshToken, host);
        }

        /// <summary>
        /// Revokes access token.
        /// </summary>
        /// <param name="host">Host which will be used to create a rest client.</param>
        /// <returns>True if revoke operation succedes; otherwise false.</returns>
        public NdRevokeTokenResponse RevokeAccessToken(string host = null)
        {
            var accessToken = clientData.GetSavedAccessToken(clientData.GetClientId());
            return RevokeToken(accessToken, TokenTypeHint.AccessToken, host);
        }

        /// <summary>
        /// Revokes boths access and refresh tokens.
        /// </summary>
        /// <param name="host">Host which will be used to create a rest client.</param>
        /// <returns>True if revoke operation succedes; otherwise false.</returns>
        public bool RevokeTokens(string host = null)
            => RevokeRefreshToken(host).Revoked
            && RevokeAccessToken(host).Revoked;

        /// <summary>
        /// Refreshes OAuth tokens.
        /// </summary>
        public void RefreshTokens()
        {
            if (string.IsNullOrEmpty(clientData.GetSavedRefreshToken(clientData.GetClientId())))
            {
                ObtainTokensByCode();
            }
            else
            {
                ObtainTokensByRefreshToken();
            }
        }

        private NdRevokeTokenResponse RevokeToken(string token, TokenTypeHint hint, string host = null)
        {
            var accessToken = clientData.GetSavedAccessToken(clientData.GetClientId());
            return oAuth.RevokeToken(accessToken, token, hint, host)
                        .ToResultOrException();
        }

        private async Task<string> GetAuthorizationCodeAsync()
        {
            var oAuthURI = string.Format(REQUEST_OAUTH_AUTHORIZATION_URL_FORMAT,
                                         clientData.HostName,
                                         clientData.GetClientId(),
                                         VALID_REDIRECT_URI);

            using (var httpClient = HttpClientFactory.Create(ProxySettings.GetProxyHandler(WebRequest.DefaultWebProxy)))
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(oAuthURI)))
            {
                var sessionCookie = await GetSessionCookieAsync(httpClient).ConfigureAwait(false);

                if (string.IsNullOrEmpty(sessionCookie))
                {
                    throw new HttpRequestException("Couldn't get session cookie.");
                }

                request.Headers.Add(COOKIE_HEADER_NAME, sessionCookie);

                var response = await httpClient.SendAsync(request)
                                               .ConfigureAwait(false);

                var authorizationFormBody = await response.Content
                                                          .ReadAsStringAsync()
                                                          .ConfigureAwait(false);

                var xt = ExtractXTCode(authorizationFormBody);

                var authorizeUrl = string.Format(AUTHORIZE_CLIENT_REQUEST_FORMAT,
                                                 clientData.HostName,
                                                 xt,
                                                 clientData.GetClientId());

                using (var authorizeRequest = new HttpRequestMessage(HttpMethod.Get, authorizeUrl))
                {
                    authorizeRequest.Headers.Add(COOKIE_HEADER_NAME, sessionCookie);
                    authorizeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JSON_MEDIATYPE));

                    var authorizationResponse = await httpClient.SendAsync(authorizeRequest)
                                                                .ConfigureAwait(false);

                    var authorization = await authorizationResponse.Content
                                                                   .ReadAsAsync<AuthorizationResponse>()
                                                                   .ConfigureAwait(false);

                    return authorization.Extra;
                }
            }
        }

        private async Task<string> GetSessionCookieAsync(HttpClient httpClient)
        {
            using (var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", userInfo.UserName),
                new KeyValuePair<string, string>("password", userInfo.Password)
            }))
            {
                var response = await httpClient.PostAsync(new Uri(string.Format(REQUEST_LOGIN_FORM_FORMAT,
                                                                                clientData.HostName)),
                                                          content);

                var location = response.Headers.Location?.ToString();

                if (string.IsNullOrEmpty(location))
                {
                    throw new HttpRequestException("Please check your credentials.");
                }

                return response.Headers
                               .GetValues("Set-Cookie")
                               .FirstOrDefault();
            }
        }

        private string ExtractXTCode(string authorizationFormBody)
            => xtCodeRegex.Match(authorizationFormBody).Groups["xtCode"].Value;
    }
}