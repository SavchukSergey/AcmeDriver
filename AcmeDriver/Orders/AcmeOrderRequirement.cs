using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeOrderRequirement {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public AcmeOrderRequirementStatus Status { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

    }
}
