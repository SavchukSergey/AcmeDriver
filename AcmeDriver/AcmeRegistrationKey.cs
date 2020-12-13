using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeRegistrationKey {

        [JsonPropertyName("kty")]
        public string Type { get; set; }

        [JsonPropertyName("n")]
        public string Modulus { get; set; }

        [JsonPropertyName("e")]
        public string Exponent { get; set; }

    }
}
