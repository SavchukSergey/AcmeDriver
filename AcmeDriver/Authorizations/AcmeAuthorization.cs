using System;
using System.Linq;

namespace AcmeDriver {
	public class AcmeAuthorization {

		public AcmeIdentifier Identifier { get; }

		public AcmeAuthorizationStatus Status { get; }

		public DateTimeOffset Expires { get; }

		public AcmeChallenge[] Challenges { get; }

		public bool Wildcard { get; }

		public Uri Location { get; }

		public AcmeAuthorization(AcmeAuthorizationData data, AcmeClientRegistration registration) {
			Identifier = data.Identifier;
			Status = data.Status;
			Expires = data.Expires;
			Challenges = data.Challenges.Select<AcmeChallengeData, AcmeChallenge>(challengeData => {
				return challengeData.Type switch {
					"http-01" => new AcmeHttp01Challenge(challengeData, this, registration),
					"dns-01" => new AcmeDns01Challenge(challengeData, this, registration),
					_ => new AcmeUnknownChallenge(challengeData, this, registration)
				};
			}).ToArray();
			Wildcard = data.Wildcard;
			Location = data.Location;
		}

	}
}
