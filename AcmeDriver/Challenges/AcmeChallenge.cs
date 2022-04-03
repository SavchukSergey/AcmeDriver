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

	}
}
