using System;
using System.Text;
using AcmeDriver.JWK;
using AcmeDriver.Utils;

namespace AcmeDriver {
	public class AcmeClientRegistration {

		public PrivateJsonWebKey Key { get; }

		public Uri Location { get; }

		public AcmeClientRegistration(PrivateJsonWebKey key, Uri location) {
			Key = key;
			Location = location;
		}

		public string GetJwkThumbprint() {
			return Key.GetPublicJwk().GetJwkThumbprint();
		}

		public string SignKid(Uri url, string nonce, byte[] payload) {
			var protectedHeader = new {
				nonce = nonce,
				url = url.ToString(),
				alg = GetSignatureAlg(),
				kid = Location.ToString()
			};
			var protectedHeaderJson = AcmeJson.Serialize(protectedHeader);
			var protectedHeaderData = Encoding.UTF8.GetBytes(protectedHeaderJson);
			var protectedHeaderEncoded = Base64Url.Encode(protectedHeaderData);

			var payloadEncoded = Base64Url.Encode(payload);

			var tbs = protectedHeaderEncoded + "." + payloadEncoded;

			var json = new {
				payload = payloadEncoded,
				@protected = protectedHeaderEncoded,
				signature = ComputeSignature(Encoding.UTF8.GetBytes(tbs))
			};
			return AcmeJson.Serialize(json);
		}

		public string Sign(Uri url, string nonce, byte[] payload) {
			var protectedHeader = new {
				nonce = nonce,
				url = url.ToString(),
				alg = GetSignatureAlg(),
				jwk = (object)Key.GetPublicJwk()
			};
			var protectedHeaderJson = AcmeJson.Serialize(protectedHeader);
			var protectedHeaderData = Encoding.UTF8.GetBytes(protectedHeaderJson);
			var protectedHeaderEncoded = Base64Url.Encode(protectedHeaderData);

			var payloadEncoded = Base64Url.Encode(payload);

			var tbs = protectedHeaderEncoded + "." + payloadEncoded;

			var json = new {
				payload = payloadEncoded,
				@protected = protectedHeaderEncoded,
				signature = ComputeSignature(Encoding.UTF8.GetBytes(tbs))
			};
			return AcmeJson.Serialize(json);
		}

		private string ComputeSignature(byte[] data) {
			var signature = Key.SignData(data);
			return Base64Url.Encode(signature);
		}

		private string GetSignatureAlg() {
			return Key.SignatureAlgorithmName;
		}

	}
}
