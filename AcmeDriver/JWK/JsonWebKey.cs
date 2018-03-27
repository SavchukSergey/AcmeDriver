using Newtonsoft.Json;
using System.Security.Cryptography;

namespace AcmeDriver.JWK {
    public abstract class JsonWebKey {

        protected static readonly SHA256 _sha256 = SHA256.Create();

        [JsonProperty("kty")]
        public abstract string Kty { get; }

    }
}
