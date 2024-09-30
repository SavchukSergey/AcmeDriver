using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeExceptionInfo {

        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;


        [JsonPropertyName("detail")]
        public string Detail { get; set; } = default!;


        [JsonPropertyName("status")]
        public int Status { get; set; }

    }

}
