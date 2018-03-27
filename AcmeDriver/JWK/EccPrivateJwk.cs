#if NETCOREAPP2_0

using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AcmeDriver.JWK {
    public class EccPrivateJwk : PrivateJsonWebKey {

        [JsonProperty("crv")]
        public string Curve { get; set; }

        [JsonProperty("x")]
        public string X { get; set; }

        [JsonProperty("y")]
        public string Y { get; set; }

        [JsonProperty("d")]
        public string D { get; set; }

        public override string Kty => "EC";

        public static EccPrivateJwk Create() {
            ECCurve curve = ECCurve.NamedCurves.nistP256;
            var ecdsa = ECDsa.Create(curve);
            var args = ecdsa.ExportParameters(true);
            return From(args);
        }

        public static EccPrivateJwk From(ECParameters privateKey) {
            return new EccPrivateJwk {
                Curve = "P-256",
                X = Base64Url.Encode(privateKey.Q.X),
                Y = Base64Url.Encode(privateKey.Q.Y),
                D = Base64Url.Encode(privateKey.D)
            };
        }

        public override PublicJsonWebKey GetPublicJwk() {
            return new EccPublicJwk {
                Curve = Curve,
                X = X,
                Y = Y
            };
        }

        public override byte[] SignData(byte[] data) {
            var curve = ECCurve.NamedCurves.nistP256;
            var args = new ECParameters {
                Curve = curve,
                D = Base64Url.Decode(D),
                Q = new ECPoint {
                    X = Base64Url.Decode(X),
                    Y = Base64Url.Decode(Y)
                }
            };

            var ecdsa = ECDsa.Create(args);
            return ecdsa.SignData(data, HashAlgorithmName.SHA256);
        }

    }
}

#endif