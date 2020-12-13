using System;
using System.Numerics;
using System.Security.Cryptography;

namespace AcmeDriver {
    public static class ECUtils {

        public static ECCurve GetCurve(string curveName) {
            return curveName switch {
                "P-256" => ECCurve.NamedCurves.nistP256,
                "P-384" => ECCurve.NamedCurves.nistP384,
                "P-521" => ECCurve.NamedCurves.nistP521,
                _ => throw new NotSupportedException($"Unknown curve {curveName}")
            };
        }

        public static string GetFipsCurveName(ECCurve curve) {
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