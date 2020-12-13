using System;
using System.Security.Cryptography;

namespace AcmeDriver.Utils {
    public static class PrivateKeyUtils {

        public static AsymmetricAlgorithm ReadPrivateKey(string pem) {
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