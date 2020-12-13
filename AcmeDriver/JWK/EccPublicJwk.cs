using AcmeDriver.Utils;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AcmeDriver.JWK {
    public class EccPublicJwk : PublicJsonWebKey {

        [JsonPropertyName("crv")]
        public string Curve { get; set; }

        [JsonPropertyName("x")]
        public string X { get; set; }

        [JsonPropertyName("y")]
        public string Y { get; set; }

        public override string Kty => "EC";

        protected override string GetJwkThumbprintJson() {
            var crv = AcmeJson.Serialize(Curve);
            var kty = AcmeJson.Serialize(Kty);
            var x = AcmeJson.Serialize(X);
            var y = AcmeJson.Serialize(Y);
            return $"{{\"crv\":{crv},\"kty\":{kty},\"x\":{x},\"y\":{y}}}";
        }

        public static EccPublicJwk From(ECParameters publicKey) {
            return new EccPublicJwk {
                Curve = ECUtils.GetFipsCurveName(publicKey.Curve),
                X = Base64Url.Encode(publicKey.Q.X),
                Y = Base64Url.Encode(publicKey.Q.Y)
            };
        }

    }
}
