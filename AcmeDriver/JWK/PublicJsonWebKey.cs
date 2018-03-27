using System.Text;

namespace AcmeDriver.JWK {
    public abstract class PublicJsonWebKey : JsonWebKey {

        //https://tools.ietf.org/html/rfc7638
        public string GetJwkThumbprint() {
            var str = GetJwkThumbprintJson();
            var hash = _sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Base64Url.Encode(hash);
        }

        protected abstract string GetJwkThumbprintJson();

    }
}
