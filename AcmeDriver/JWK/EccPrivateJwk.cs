#if (NETCOREAPP2_0 || NETCOREAPP2_1)

using Newtonsoft.Json;
using System;
using System.Numerics;
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
                Curve = GetFipsCurveName(privateKey.Curve),
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
            var curve = GetCurve();
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

        private static string GetFipsCurveName(ECCurve curve) {
            if (AreCurvesEqual(curve, ECCurve.NamedCurves.nistP256)) {
                return "P-256";
            }
            if (AreCurvesEqual(curve, ECCurve.NamedCurves.nistP384)) {
                return "P-384";
            }
            if (AreCurvesEqual(curve, ECCurve.NamedCurves.nistP521)) {
                return "P-521";
            }
            return "unknown-curve";
        }

        private static bool AreCurvesEqual(ECCurve a, ECCurve b) {
            if (!string.IsNullOrWhiteSpace(a.Oid?.FriendlyName) && !string.IsNullOrWhiteSpace(b.Oid?.FriendlyName)) {
                if (a.Oid.FriendlyName == b.Oid.FriendlyName) return true;
            }
            if (!string.IsNullOrWhiteSpace(a.Oid?.Value) && !string.IsNullOrWhiteSpace(b.Oid?.Value)) {
                if (a.Oid.Value == b.Oid.Value) return true;
            }
            if (HasExplicitData(a) && HasExplicitData(b)) {
                if (new BigInteger(a.A) != new BigInteger(b.A)) {
                    return false;
                }
                if (new BigInteger(a.B) != new BigInteger(b.B)) {
                    return false;
                }
                if (new BigInteger(a.Cofactor) != new BigInteger(b.Cofactor)) {
                    return false;
                }
                if (new BigInteger(a.Order) != new BigInteger(b.Order)) {
                    return false;
                }
                if (new BigInteger(a.G.X) != new BigInteger(b.G.X)) {
                    return false;
                }
                if (new BigInteger(a.G.Y) != new BigInteger(b.G.Y)) {
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool HasExplicitData(ECCurve curve) {
            if (curve.A == null || curve.A.Length == 0) return false;
            if (curve.B == null || curve.B.Length == 0) return false;
            if (curve.Cofactor == null || curve.Cofactor.Length == 0) return false;
            if (curve.Order == null || curve.Order.Length == 0) return false;
            if (curve.G.X == null || curve.G.X.Length == 0) return false;
            if (curve.G.Y == null || curve.G.Y.Length == 0) return false;
            return true;
        }

    }
}

#endif