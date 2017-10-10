using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeOrder {

        [JsonProperty("csr")]
        public string Csr { get; set; }

        [JsonProperty("notBefore")]
        public DateTimeOffset NotBefore { get; set; }

        [JsonProperty("notAfter")]
        public DateTimeOffset NotAfter { get; set; }

        [JsonProperty("status")]
        public AcmeOrderStatus Status { get; set; }

        [JsonProperty("expires")]
        public DateTimeOffset Expries { get; set; }

        public Uri Location { get; set; }

    }
}
