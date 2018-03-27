using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeChallengeData {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public string Uri { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("error")]
        public AcmeExceptionInfo Error { get; set; }

        public string GetKeyAuthorization(AcmeClientRegistration reg) {
            return $"{Token}.{reg.GetJwkThumbprint()}";
        }

    }
}
