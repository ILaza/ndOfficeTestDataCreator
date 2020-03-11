using System.Collections.Generic;

namespace NetDocuments.Automation.Common.Settings
{
    // TODO: need to be refactored.
    /// <summary>
    /// Class holds info about hosts.
    /// Types of hosts: Vault, Preview.
    /// </summary>
    public class HostSettings
    {
        /// <summary>
        /// Gets base URL string in format "https://{HostName}/";
        /// </summary>
        public string BaseUrl => $"https://{HostName}";

        /// <summary>
        /// Name of vault host.
        /// </summary>
        public const string VAULT_HOST = "vault";

        /// <summary>
        /// Full name of vault host.
        /// </summary>
        public const string VAULT_HOST_FULL_NAME = "vault.netvoyage.com";

        /// <summary>
        /// Name of ducot host.
        /// </summary>
        public const string DUCOT_HOST = "ducot";

        /// <summary>
        /// Name of eu host.
        /// </summary>
        public const string EU_HOST = "eu";

        /// <summary>
        /// Name of au host.
        /// </summary>
        public const string AU_HOST = "au";

        /// <summary>
        /// Full name of ducot host.
        /// </summary>
        public const string DUCOT_HOST_FULL_NAME = "ducot.netdocuments.com";

        /// <summary>
        /// Host name.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// URL the client was registered for.
        /// </summary>
        public string ApiUrl { get; private set; }

        /// <summary>
        /// API host name.
        /// </summary>
        public string ApiHostName { get; private set; }

        /// <summary>
        /// Client Id.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Secret Id.
        /// </summary>
        public string SecretId { get; private set; }

        /// <summary>
        /// Redirect URL.
        /// </summary>
        public string RedirectUri { get; private set; }

        private static readonly Dictionary<string, HostSettings> HostsSettingDictionary = new Dictionary<string, HostSettings>
        {
            {
                VAULT_HOST,
                new HostSettings(
                    "api.vault.netvoyage.com",
                    "vault.netvoyage.com",
                    "https://api.vault.netvoyage.com",
                    "AP-OF0BSK2V",
                    "1afgqNfRRecLCn8g656WzhiwIBAdT9UfdUgCXtq502guRE2A",
                    "https://localhost/")
            },
            {
                DUCOT_HOST,
                new HostSettings(
                    "api.ducot.netdocuments.com",
                    "ducot.netdocuments.com",
                    "https://api.ducot.netdocuments.com",
                    "AP-BD15GCDS",
                    "n9xbjBqraJ9iBuJIY8eQy59a9rRI7tm26oluLUkkNMMxzSBe",
                    "https://localhost/")
            },
            {
                EU_HOST,
                new HostSettings(
                    "api.eu.netdocuments.com",
                    "eu.netdocuments.com",
                    "https://api.eu.netdocuments.com/",
                    "AP-9L3NZFJO",
                    "iJ5RGF9eXXr86CFzl5kGbqEfT2RUIZf7GhXI8eHld1Km4gPY",
                    "https://localhost/")
            },
            {
                AU_HOST,
                new HostSettings(
                    "api.au.netdocuments.com",
                    "au.netdocuments.com",
                    "https://api.au.netdocuments.com/",
                    "AP-VF2M36ME",
                    "4hZNwT22s9HCyrqXEQlGXK0OrsCKpcD9UOBE7nck9H4fzAgI",
                    "https://localhost/")
            }
        };

        /// <summary>
        /// Constructs client info from URL the client was registered for.
        /// </summary>
        /// <param name="apiHostName">API host name.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="apiUrl">URL the client was registered for.</param>
        /// <param name="clientId">Client Id.</param>
        /// <param name="secretId">Secret Id.</param>
        /// <param name="redirectUri">Redirect URL.</param>
        public HostSettings(string apiHostName, string hostName, string apiUrl, string clientId, string secretId, string redirectUri)
        {
            HostName = hostName;
            ApiHostName = apiHostName;
            ApiUrl = apiUrl;
            ClientId = clientId;
            SecretId = secretId;
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Gets host settings for the given host name.
        /// </summary>
        /// <returns>
        /// Returns predefines host setting for the given host name. Returns null for unknown host name.
        /// </returns>
        /// <param name="hostName">Name of the host.</param>
        public static HostSettings GetHostSettings(string hostName)
        {
            HostSettings hostSettings;
            HostsSettingDictionary.TryGetValue(hostName, out hostSettings);
            return hostSettings;
        }

        /// <summary>
        /// Gets default host settings. By default, ducot.
        /// </summary>
        /// <returns></returns>
        public static HostSettings GetDefualtHostSettings()
            => GetHostSettings(DUCOT_HOST);
    }
}