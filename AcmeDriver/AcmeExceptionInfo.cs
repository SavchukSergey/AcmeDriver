using System;
using Newtonsoft.Json;

namespace AcmeDriver {

    public class AcmeExceptionInfo {

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("detail")]
        public string Detail { get; set; }


        [JsonProperty("status")]
        public int Status { get; set; }

    }

}
