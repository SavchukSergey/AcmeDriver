using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AcmeDriver.JWK {
    public class RsaPrivateJwk : PrivateJsonWebKey {

        [JsonProperty("n")]
        public string Modulus { get; set; }

        [JsonProperty("e")]
        public string Exponent { get; set; }

        [JsonProperty("d")]
        public string D { get; set; }

        [JsonProperty("p")]
        public string P { get; set; }

        [JsonProperty("q")]
        public string Q { get; set; }

        [JsonProperty("dp")]
        public string DP { get; set; }

        [JsonProperty("dq")]
        public string DQ { get; set; }

        [JsonProperty("qi")]
        public string InverseQ { get; set; }

        public override string Kty => "RSA";

        public static RsaPrivateJwk Create() {
            var rsa = RSA.Create();
            rsa.KeySize = 2048;
            var args = rsa.ExportParameters(true);
            return From(args);
        }

        public static RsaPrivateJwk From(RSAParameters parameters) {
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
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

    }
}
