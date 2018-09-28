#if (NETCOREAPP2_0 || NETCOREAPP2_1)

using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AcmeDriver.JWK {
    public class EccPublicJwk : PublicJsonWebKey {

        [JsonProperty("crv")]
        public string Curve { get; set; }

        [JsonProperty("x")]
        public string X { get; set; }

        [JsonProperty("y")]
        public string Y { get; set; }

        public override string Kty => "EC";

        protected override string GetJwkThumbprintJson() {
            var crv = JsonConvert.SerializeObject(Curve);
            var kty = JsonConvert.SerializeObject(Kty);
            var x = JsonConvert.SerializeObject(X);
            var y = JsonConvert.SerializeObject(Y);
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

#endif