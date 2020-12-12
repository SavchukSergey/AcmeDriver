using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeOrder {

        [JsonProperty("status")]
        public AcmeOrderStatus Status { get; set; }

        [JsonProperty("expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonProperty("identifiers")]
        public AcmeIdentifier[] Identifiers { get; set; }

        [JsonProperty("authorizations")]
        public string[] Authorizations { get; set; }

        [JsonProperty("finalize")]
        public string Finalize { get; set; }

        [JsonProperty("certificate")]
        public string? Certificate { get; set; }

        [JsonIgnore]
        public Uri Location { get; set; }

    }
}
