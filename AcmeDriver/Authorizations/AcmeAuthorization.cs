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
			Challenges = data.Challenges
				.Select(challengeData => AcmeChallenge.From(challengeData, this, registration))
				.ToArray();
			Wildcard = data.Wildcard;
			Location = data.Location;
		}

	}
}
