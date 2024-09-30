using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeOrderRequirement {

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        [JsonPropertyName("status")]
        public AcmeOrderRequirementStatus Status { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = default!;

    }
}
