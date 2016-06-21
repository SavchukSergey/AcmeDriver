using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeDirectory {

        [JsonProperty("new-authz")]
        public string NewAuthorizationUrl { get; set; }

        [JsonProperty("new-cert")]
        public string NewCertificateUrl { get; set; }

        [JsonProperty("new-reg")]
        public string NewRegistrationUrl { get; set; }

        [JsonProperty("revoke-cert")]
        public string RevokeCertificateUrl { get; set; }

    }
}
