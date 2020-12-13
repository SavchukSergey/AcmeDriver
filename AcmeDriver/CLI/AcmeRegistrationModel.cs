using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AcmeDriver.JWK;

namespace AcmeDriver.CLI {
    public class AcmeRegistrationModel {

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("key")]
        public PrivateJsonWebKey Key { get; set; }

        [JsonPropertyName("contacts")]
        public IList<string> Contacts { get; set; }

        [JsonPropertyName("location")]
        public Uri Location { get; set; }

    }
}