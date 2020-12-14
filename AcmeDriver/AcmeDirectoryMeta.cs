using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeDirectoryMeta {

        [JsonPropertyName("caaIdentities")]
        public string[]? CaaIdentities { get; set; }

        [JsonPropertyName("termsOfService")]
        public Uri? TermsOfService { get; set; }

        [JsonPropertyName("website")]
        public Uri? WebSite { get; set; }

    }
}
