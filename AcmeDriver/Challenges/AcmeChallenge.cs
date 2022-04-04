using System.Threading.Tasks;

namespace AcmeDriver {
	public abstract class AcmeChallenge {

		public string Domain { get; }

		public AcmeChallengeData Data { get; }

		public AcmeAuthorization Authorization { get; }

		public AcmeClientRegistration Registration { get; }

		public AcmeChallenge(AcmeChallengeData data, AcmeAuthorization authorization, AcmeClientRegistration registration) {
			Data = data;
			Authorization = authorization;
			Registration = registration;
			Domain = authorization.Identifier.Value;
		}

		public abstract Task<bool> PrevalidateAsync();

		public static AcmeChallenge From(AcmeChallengeData challengeData, AcmeAuthorization authz, AcmeClientRegistration registration) {
			return challengeData.Type switch {
				"http-01" => new AcmeHttp01Challenge(challengeData, authz, registration),
				"dns-01" => new AcmeDns01Challenge(challengeData, authz, registration),
				_ => new AcmeUnknownChallenge(challengeData, authz, registration)
			};
		}

	}
}
