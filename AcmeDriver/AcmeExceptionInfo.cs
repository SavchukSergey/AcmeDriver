using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeExceptionInfo {

        [JsonPropertyName("type")]
        public string Type { get; set; }


        [JsonPropertyName("detail")]
        public string Detail { get; set; }


        [JsonPropertyName("status")]
        public int Status { get; set; }

    }

}
