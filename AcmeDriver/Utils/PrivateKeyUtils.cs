using System;
using System.Security.Cryptography;
using AcmeDriver.JWK;

namespace AcmeDriver.Utils {
    public static class PrivateKeyUtils {

        public static string ToPem(PrivateJsonWebKey key) {
            var algo = key.CreateAsymmetricAlgorithm();
            return ToPem(algo);
        }

        public static string ToPem(AsymmetricAlgorithm algorithm) {
            return algorithm switch {
                RSA rsa => PemUtils.DERtoPEM(rsa.ExportRSAPrivateKey(), "RSA PRIVATE KEY"),
                ECDsa ecdsa => PemUtils.DERtoPEM(ecdsa.ExportECPrivateKey(), "EC PRIVATE KEY"),
                _ => throw new NotSupportedException($"Private key is not supported ({algorithm.GetType().FullName})")
            };
        }

        public static PrivateJsonWebKey ToPrivateJsonWebKey(string pem) {
            var algo = ToAsymmetricAlgorithm(pem);
            return ToPrivateJsonWebKey(algo);
        }

        public static PrivateJsonWebKey ToPrivateJsonWebKey(AsymmetricAlgorithm algorithm) {
            return algorithm switch {
                RSA rsa => RsaPrivateJwk.From(rsa.ExportParameters(true)),
                ECDsa ecdsa => EccPrivateJwk.From(ecdsa.ExportParameters(true)),
                _ => throw new NotSupportedException($"Private key is not supported ({algorithm.GetType().FullName})")
            };
        }

        public static AsymmetricAlgorithm ToAsymmetricAlgorithm(string pem) {
            if (pem.Contains("RSA PRIVATE KEY")) {
                var cryptoServiceProvider = new RSACryptoServiceProvider();
                cryptoServiceProvider.ImportFromPem(pem);
                return cryptoServiceProvider;
            }
            if (pem.Contains("EC PRIVATE KEY")) {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(pem);
                return ecdsa;
            }
            throw new NotSupportedException("This kind of private key is not supported");
        }

    }
}