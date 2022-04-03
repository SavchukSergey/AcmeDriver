using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeChallengeData {

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("url")]
        public Uri Url { get; set; } = default!;

        [JsonPropertyName("token")]
        public string Token { get; set; } = default!;
      
        [JsonPropertyName("validated")]
        public string? Validated { get; set; }

        [JsonPropertyName("error")]
        public AcmeExceptionInfo? Error { get; set; }

        public string GetKeyAuthorization(AcmeClientRegistration reg) {
            return $"{Token}.{reg.GetJwkThumbprint()}";
        }

    }
}
