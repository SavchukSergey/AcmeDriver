using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AcmeDriver.JWK {
    public abstract class JsonWebKey {

        protected static readonly SHA256 _sha256 = SHA256.Create();

        [JsonPropertyName("kty")]
        public abstract string Kty { get; }

    }
}
