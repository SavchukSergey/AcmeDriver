using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeRegistration : AcmeResource {

        [JsonPropertyName("key")]
        public AcmeRegistrationKey Key { get; set; } = null!;

        [JsonPropertyName("contact")]
        public string[] Contacts { get; set; } = null!;

        [JsonPropertyName("initialIp")]
        public string InitialIp { get; set; } = null!;

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("agreement")]
        public string Agreement { get; set; } = null!;

    }
}
