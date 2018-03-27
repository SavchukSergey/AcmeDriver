using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AcmeDriver.JWK {
    public class RsaPublicJwk : PublicJsonWebKey {

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        public override string Kty => "RSA";

        protected override string GetJwkThumbprintJson() {
            var n = JsonConvert.SerializeObject(Modulus);
            var e = JsonConvert.SerializeObject(Exponent);
            var kty = JsonConvert.SerializeObject(Kty);
            return $"{{\"e\":{e},\"kty\":{kty},\"n\":{n}}}";
        }

        public static RsaPublicJwk From(RSAParameters parameters) {
            return new RsaPublicJwk {
                Modulus = Base64Url.Encode(parameters.Modulus),
                Exponent = Base64Url.Encode(parameters.Exponent)
            };
        }

    }
}
