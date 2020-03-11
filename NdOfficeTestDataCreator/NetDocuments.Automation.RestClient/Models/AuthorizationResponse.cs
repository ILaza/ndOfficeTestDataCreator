using Newtonsoft.Json;

namespace NetDocuments.Automation.RestClient.Models
{
    /// <summary>
    /// Represents authorization response message.
    /// </summary>
    public class AuthorizationResponse
    {
        [JsonProperty("errorNumber")]
        public int ErrorNumber { get; set; }

        [JsonProperty("errorText")]
        public string ErrorText { get; set; }

        [JsonProperty("extra")]
        public string Extra { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
