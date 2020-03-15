using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeRegistrationKey {

        [JsonProperty("kty")]
        public string Type { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

    }
}
