using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeChallenge {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("validated")]
        public DateTimeOffset? Validated { get; set; }

        public object GetResponse() {
            switch (Type) {
                case "dns-01":
                    return new {
                        type = "dns-01",
                        token = Token
                    };
                default:
                    throw new NotSupportedException($"{Type} challenge type is not supported");
            }
        }

        public string GetKeyAythorization(AcmeClientRegistration reg) {
            return $"{Token}.{reg.GetJwkThumbprint()}";
        }

    }
}
