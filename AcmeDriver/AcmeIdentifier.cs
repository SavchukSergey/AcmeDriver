using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeIdentifier {

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = default!;

        public override string ToString() {
            return $"{Type}:{Value}";
        }

    }
}
