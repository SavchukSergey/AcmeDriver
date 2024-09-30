using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeAuthorizationData : AcmeResource {

        [JsonPropertyName("identifier")]
        public AcmeIdentifier Identifier { get; set; } = default!;

        [JsonPropertyName("status")]
        public AcmeAuthorizationStatus Status { get; set; }

        [JsonPropertyName("expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonPropertyName("challenges")]
        public AcmeChallengeData[] Challenges { get; set; } = default!;

        [JsonPropertyName("wildcard")]
        public bool Wildcard { get; set; }

    }
}
