using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeUnknownChallenge : AcmeChallenge {

		public AcmeUnknownChallenge(AcmeChallengeData data, AcmeAuthorization authorization, AcmeClientRegistration registration) : base(data, authorization, registration) {
		}

		public override Task<bool> PrevalidateAsync() {
			return Task.FromResult(false);
		}

	}
}