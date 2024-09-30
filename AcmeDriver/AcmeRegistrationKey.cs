using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeRegistrationKey {

        [JsonPropertyName("kty")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("n")]
        public string Modulus { get; set; } = default!;

        [JsonPropertyName("e")]
        public string Exponent { get; set; } = default!;

    }
}
