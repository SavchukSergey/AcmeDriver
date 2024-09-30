using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeOrder {

        [JsonPropertyName("status")]
        public AcmeOrderStatus Status { get; set; }

        [JsonPropertyName("expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonPropertyName("identifiers")]
        public AcmeIdentifier[] Identifiers { get; set; } = default!;

        [JsonPropertyName("authorizations")]
        public Uri[] Authorizations { get; set; } = default!;

        [JsonPropertyName("finalize")]
        public Uri Finalize { get; set; } = default!;

        [JsonPropertyName("certificate")]
        public Uri? Certificate { get; set; }

        [JsonIgnore]
        public Uri Location { get; set; } = default!;

    }
}
