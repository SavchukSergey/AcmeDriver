using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeChallengeData {

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("url")]
        public string Uri { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("error")]
        public AcmeExceptionInfo Error { get; set; }

        public string GetKeyAuthorization(AcmeClientRegistration reg) {
            return $"{Token}.{reg.GetJwkThumbprint()}";
        }

    }
}
