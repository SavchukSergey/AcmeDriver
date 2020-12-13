using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeIdentifier {

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        public override string ToString() {
            return $"{Type}:{Value}";
        }

    }
}
