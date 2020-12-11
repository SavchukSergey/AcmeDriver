using System;
using System.Collections.Generic;
using AcmeDriver.JWK;
using Newtonsoft.Json;

namespace AcmeDriver.CLI {
    public class AcmeRegistrationModel {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("key")]
        public PrivateJsonWebKey Key { get; set; }

        [JsonProperty("contacts")]
        public IList<string> Contacts { get; set; }

        [JsonProperty("location")]
        public Uri Location { get; set; }

    }
}