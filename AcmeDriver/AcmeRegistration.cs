using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeRegistration : AcmeResource {

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("key")]
        public AcmeRegistrationKey Key { get; set; }

        [JsonPropertyName("contact")]
        public string[] Contacts { get; set; }

        [JsonPropertyName("initialIp")]
        public string InitialIp { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("agreement")]
        public string Agreement { get; set; }

    }
}
