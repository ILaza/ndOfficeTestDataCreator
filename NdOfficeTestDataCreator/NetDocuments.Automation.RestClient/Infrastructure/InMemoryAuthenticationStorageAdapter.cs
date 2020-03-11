using System;
using System.Collections.Concurrent;

using NetDocuments.Client.Common.Contracts;
using NetDocuments.Client.Common.Contracts.Interfaces;
using NetDocuments.Rest.Contracts.Enums;
using NetDocuments.Rest.Contracts.Models.V1;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    public class InMemoryAuthenticationStorageAdapter : IAuthenticationStorageAdapter
    {
        private const int EXPIRATION_DATE_CORRECTION = -2;

        private readonly object locker;
        private readonly ConcurrentDictionary<string, (TokenInfo accessTokenInfo, TokenInfo refreshTokenInfo)> tokens;
        
        private string currentUserName;

        public InMemoryAuthenticationStorageAdapter()
        {
            locker = new object();
            tokens = new ConcurrentDictionary<string, (TokenInfo accessToken, TokenInfo refreshToken)>();
        }

        public void ClearAccessTokensData()
        {
            lock (locker)
            {
                tokens.Clear();
            }
        }

        public string GetAccessToken(string key)
        {
            ValidateKey(key);

            currentUserName = key;

            if (tokens.TryGetValue(key, out (TokenInfo accessTokenInfo, TokenInfo refreshTokenInfo) tokensInfo))
            {
                var (accessTokenInfo, _) = tokensInfo;
                return accessTokenInfo.Token;
            }

            return null;
        }

        public DateTime GetAccessTokenExpirationDate(string key)
        {
            ValidateKey(key);

            currentUserName = key;

            if (tokens.TryGetValue(key, out (TokenInfo accessTokenInfo, TokenInfo refreshTokenInfo) tokensInfo))
            {
                var (accessTokenInfo, _) = tokensInfo;
                return accessTokenInfo.TokenExpirationDate ?? DateTime.Now.AddMinutes(-1);
            }

            return DateTime.Now.AddMinutes(-1);
        }

        public string GetRefreshToken(string key)
        {
            ValidateKey(key);

            currentUserName = key;

            if (tokens.TryGetValue(key, out (TokenInfo accessTokenInfo, TokenInfo refreshTokenInfo) tokensInfo))
            {
                var (_, refreshTokenInfo) = tokensInfo;
                return refreshTokenInfo.Token;
            }

            return null;
        }

        public void ResetCurrentUserInfo()
        {
            currentUserName = null;
        }

        public string RetrieveRefreshToken(string key)
        {
            return GetRefreshToken(key);
        }

        public void SaveTokens(NdRefreshTokenResponse accessTokens, string key)
        {
            lock (locker)
            {
                ValidateKey(key);

                currentUserName = key;

                var accessTokenInfo = new TokenInfo
                {
                    TokenType = TokenTypeHint.AccessToken,
                    Token = accessTokens.NullCheck(response => response.AccessToken)
                };

                var refreshTokenInfo = new TokenInfo
                {
                    TokenType = TokenTypeHint.RefreshToken,
                    Token = accessTokens.NullCheck(response => response.RefreshToken)
                };

                accessTokenInfo.TokenExpirationDate = accessTokens.NullCheck(response => GetExpirationDate(response.ExpiresIn));
                tokens.AddOrUpdate(key, (accessTokenInfo, refreshTokenInfo), (s, info) => (accessTokenInfo, refreshTokenInfo));
            }
        }

        private DateTime GetExpirationDate(int expiresIn)
        {
            return DateTime.Now
                           .AddSeconds(expiresIn)
                           .AddMinutes(EXPIRATION_DATE_CORRECTION);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Parameter can't be empty", nameof(key));
            }
        }
    }
}
