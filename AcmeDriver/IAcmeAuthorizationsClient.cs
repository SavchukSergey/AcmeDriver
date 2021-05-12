using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public interface IAcmeAuthorizationsClient {

		Task<AcmeAuthorization> NewAuthorizationAsync(AcmeIdentifier identifier);

		Task<AcmeAuthorization> NewAuthorizationAsync(string domainName);

		Task<AcmeAuthorization> GetAuthorizationAsync(Uri location);

		Task DeactivateAuthorizationAsync(Uri authorizationUri);

		Task DeleteAuthorizationAsync(Uri authorizationUri);

		Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallenge challenge);

		Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallengeData challenge);

	}
}