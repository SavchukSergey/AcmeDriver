using AcmeDriver.Utils;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AcmeDriver.JWK {
    public class RsaPublicJwk : PublicJsonWebKey {

        [JsonPropertyName("n")]
        public string Modulus { get; set; }

        [JsonPropertyName("e")]
        public string Exponent { get; set; }

        public override string Kty => "RSA";

        protected override string GetJwkThumbprintJson() {
            var n = AcmeJson.Serialize(Modulus);
            var e = AcmeJson.Serialize(Exponent);
            var kty = AcmeJson.Serialize(Kty);
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
