using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeClientRegistration {

        private static readonly SHA256 _sha256 = new SHA256CryptoServiceProvider();

        public long Id { get; set; }

        public RSA Key { get; set; }

        public object GetJwk() {
            var parameters = Key.ExportParameters(false);
            return new {
                kty = "RSA",
                n = Base64Url.Encode(parameters.Modulus),
                e = Base64Url.Encode(parameters.Exponent)
            };
        }

        //https://tools.ietf.org/html/rfc7638
        public string GetJwkThumbprint() {
            var parameters = Key.ExportParameters(false);
            var n = JsonConvert.SerializeObject(Base64Url.Encode(parameters.Modulus));
            var e = JsonConvert.SerializeObject(Base64Url.Encode(parameters.Exponent));
            var kty = JsonConvert.SerializeObject("RSA");
            var str = $"{{\"e\":{e},\"kty\":{kty},\"n\":{n}}}";
            var hash = _sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Base64Url.Encode(hash);
        }

        public XElement Export() {
            return new XElement("registration",
                new XAttribute("id", Id.ToString()),
                new XElement("key", Key.ToXmlString(true)));
        }
    }
}
