using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeAuthorization : AcmeResource {

        [JsonProperty("identifier")]
        public AcmeIdentifier Identifier { get; set; }

        [JsonProperty("status")]
        public AcmeAuthorizationStatus Status { get; set; }

        [JsonProperty("expires")]
        public DateTimeOffset Expires { get; set; }

        [JsonProperty("challenges")]
        public AcmeChallenge[] Challenges { get; set; }

        [JsonProperty("combinations")]
        public int[][] Combinations { get; set; }

    }
}
