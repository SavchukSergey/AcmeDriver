using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using AcmeDriver.Utils;

namespace AcmeDriver.JWK {
    public class EccPrivateJwk : PrivateJsonWebKey {

        [JsonPropertyName("crv")]
        public string Curve { get; set; }

        [JsonPropertyName("x")]
        public string X { get; set; }

        [JsonPropertyName("y")]
        public string Y { get; set; }

        [JsonPropertyName("d")]
        public string D { get; set; }

        public override string Kty => "EC";

        public static EccPrivateJwk Create() {
            ECCurve curve = ECCurve.NamedCurves.nistP256;
            var ecdsa = ECDsa.Create(curve);
            var args = ecdsa.ExportParameters(true);
            return From(args);
        }

        public static EccPrivateJwk From(ECParameters privateKey) {
            if (privateKey.Q.X == null) {
                throw new ArgumentNullException(nameof(privateKey.Q.X));
            }
            if (privateKey.Q.Y == null) {
                throw new ArgumentNullException(nameof(privateKey.Q.Y));
            }
            if (privateKey.D == null) {
                throw new ArgumentNullException(nameof(privateKey.D));
            }
            return new EccPrivateJwk {
                Curve = ECUtils.GetFipsCurveName(privateKey.Curve),
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
            var ecdsa = CreateEcdsa();
            return ecdsa.SignData(data, HashAlgorithmName);
        }

        private ECCurve GetCurve() {
            switch (Curve) {
                case "P-256":
                    return ECCurve.NamedCurves.nistP256;
                case "P-384":
                    return ECCurve.NamedCurves.nistP384;
                case "P-521":
                    return ECCurve.NamedCurves.nistP521;
                default:
                    return ECCurve.CreateFromFriendlyName(Curve);
            }
        }

        public override AsymmetricAlgorithm CreateAsymmetricAlgorithm() {
            return CreateEcdsa();
        }

        public override string SignatureAlgorithmName {
            get {
                switch (Curve) {
                    case "P-256":
                        return "ES256";
                    case "P-384":
                        return "ES384";
                    case "P-521":
                        return "ES512";
                    default:
                        throw new NotSupportedException($"Curve {Curve} is not supported");
                }
            }
        }

        public HashAlgorithmName HashAlgorithmName {
            get {
                switch (SignatureAlgorithmName) {
                    case "ES256":
                        return HashAlgorithmName.SHA256;
                    case "ES384":
                        return HashAlgorithmName.SHA384;
                    case "ES512":
                        return HashAlgorithmName.SHA512;
                    default:
                        throw new NotSupportedException($"Hash {SignatureAlgorithmName} is not supported");
                }
            }
        }

        private ECDsa CreateEcdsa() {
            var curve = GetCurve();
            var args = new ECParameters {
                Curve = curve,
                D = Base64Url.Decode(D),
                Q = new ECPoint {
                    X = Base64Url.Decode(X),
                    Y = Base64Url.Decode(Y)
                }
            };

            return ECDsa.Create(args);
        }
    }
}