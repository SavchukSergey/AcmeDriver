using System;
using AcmeDriver.JWK;
using AcmeDriver.Utils;

namespace AcmeDriver.Tests.Challenges {
	public static class AcmeChallengeTests {

		public const string KEY_PEM = @"-----BEGIN EC PRIVATE KEY-----
MIGkAgEBBDDwYOfqpdfNZnqohtGCMX9oX5k8O3nL3wxHrW0EDrN1pIOCpxH90BRO
7QS6p6gdP+CgBwYFK4EEACKhZANiAARDXqhUyyaO1ePrDT7u7tlG4WcowLQNMPwv
qXEzv0IuuHjQqvweURG7gPEoHyF7kv/52dczgZTo6DZRrHsvZWEd863rTYRUiVeZ
l5WpxP/IhTwCHnOVtaOU9UXC1Y71CT8=
-----END EC PRIVATE KEY-----";

		public static readonly PrivateJsonWebKey Key = PrivateKeyUtils.ToPrivateJsonWebKey(KEY_PEM);

		public static readonly AcmeClientRegistration Registration = new AcmeClientRegistration(
			Key, new Uri("https://acmeclient.com")
		);

		public static readonly AcmeAuthorization Authorization = new AcmeAuthorization(
			new AcmeAuthorizationData {
				Identifier = new AcmeIdentifier {
					Value = "test.com"
				},
				Challenges = new[] {
					new AcmeChallengeData {
						Type = "dns-01",
						Token = "2ec79eb558f540bd8252db951aba897b"
					},
                    new AcmeChallengeData {
                        Type = "http-01",
                        Token = "fd20c6e3ca71455dbd76a88e19a0a4e3"
                    }
				}
			},
			new AcmeClientRegistration(
				Key, new Uri("https://acmeclient.com")
			)
		);

	}
}