using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AcmeDriver.JWK {
    public class RsaPrivateJwk : PrivateJsonWebKey {

        [JsonPropertyName("n")]
        public string Modulus { get; set; }

        [JsonPropertyName("e")]
        public string Exponent { get; set; }

        [JsonPropertyName("d")]
        public string D { get; set; }

        [JsonPropertyName("p")]
        public string P { get; set; }

        [JsonPropertyName("q")]
        public string Q { get; set; }

        [JsonPropertyName("dp")]
        public string DP { get; set; }

        [JsonPropertyName("dq")]
        public string DQ { get; set; }

        [JsonPropertyName("qi")]
        public string InverseQ { get; set; }

        public override string Kty => "RSA";

        public override string SignatureAlgorithmName => "RS256";

        public static RsaPrivateJwk Create() {
            var rsa = RSA.Create();
            rsa.KeySize = 2048;
            var args = rsa.ExportParameters(true);
            return From(args);
        }

        public static RsaPrivateJwk From(RSAParameters parameters) {
            if (parameters.Modulus == null) {
                throw new ArgumentNullException(nameof(parameters.Modulus));
            }
            if (parameters.Exponent == null) {
                throw new ArgumentNullException(nameof(parameters.Exponent));
            }
            if (parameters.D == null) {
                throw new ArgumentNullException(nameof(parameters.D));
            }
            if (parameters.P == null) {
                throw new ArgumentNullException(nameof(parameters.P));
            }
            if (parameters.Q == null) {
                throw new ArgumentNullException(nameof(parameters.Q));
            }
            if (parameters.DP == null) {
                throw new ArgumentNullException(nameof(parameters.DP));
            }
            if (parameters.DQ == null) {
                throw new ArgumentNullException(nameof(parameters.DQ));
            }
            if (parameters.InverseQ == null) {
                throw new ArgumentNullException(nameof(parameters.InverseQ));
            }
            return new RsaPrivateJwk {
                Modulus = Base64Url.Encode(parameters.Modulus),
                Exponent = Base64Url.Encode(parameters.Exponent),
                D = Base64Url.Encode(parameters.D),
                P = Base64Url.Encode(parameters.P),
                Q = Base64Url.Encode(parameters.Q),
                DP = Base64Url.Encode(parameters.DP),
                DQ = Base64Url.Encode(parameters.DQ),
                InverseQ = Base64Url.Encode(parameters.InverseQ)
            };
        }

        public RSAParameters ExportParameters() {
            return new RSAParameters {
                Modulus = Base64Url.Decode(Modulus),
                Exponent = Base64Url.Decode(Exponent),
                D = Base64Url.Decode(D),
                P = Base64Url.Decode(P),
                Q = Base64Url.Decode(Q),
                DP = Base64Url.Decode(DP),
                DQ = Base64Url.Decode(DQ),
                InverseQ = Base64Url.Decode(InverseQ)
            };
        }

        public override PublicJsonWebKey GetPublicJwk() {
            return new RsaPublicJwk {
                Modulus = Modulus,
                Exponent = Exponent
            };
        }

        public override byte[] SignData(byte[] data) {
            var rsa = CreateRSA();
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public override AsymmetricAlgorithm CreateAsymmetricAlgorithm() {
            return CreateRSA();
        }

        protected RSA CreateRSA() {
            var args = new RSAParameters {
                Modulus = Base64Url.Decode(Modulus),
                Exponent = Base64Url.Decode(Exponent),
                D = Base64Url.Decode(D),
                P = Base64Url.Decode(P),
                Q = Base64Url.Decode(Q),
                DP = Base64Url.Decode(DP),
                DQ = Base64Url.Decode(DQ),
                InverseQ = Base64Url.Decode(InverseQ)
            };
            var rsa = RSA.Create();
            rsa.ImportParameters(args);
            return rsa;
        }

    }
}
