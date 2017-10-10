using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeRegistration : AcmeResource {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("key")]
        public AcmeRegistrationKey Key { get; set; }

        [JsonProperty("contact")]
        public string[] Contacts { get; set; }

        [JsonProperty("initialIp")]
        public string InitialIp { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("agreement")]
        public string Agreement { get; set; }

    }
}
