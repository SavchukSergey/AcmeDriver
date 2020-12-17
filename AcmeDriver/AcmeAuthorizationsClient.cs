using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeAuthorizationsClient {

		private readonly AcmeAuthenticatedClientContext _context;

		public AcmeAuthorizationsClient(AcmeAuthenticatedClientContext context) {
			_context = context;
		}

		public async Task<AcmeAuthorization> NewAuthorizationAsync(AcmeIdentifier identifier) {
			return await _context.SendPostAsync<object, AcmeAuthorization>(_context.Directory.NewAuthzUrl, new {
				resource = "new-authz",
				identifier = new {
					type = identifier.Type,
					value = identifier.Value
				}
			}).ConfigureAwait(false);
		}

		public Task<AcmeAuthorization> NewAuthorizationAsync(string domainName) {
			return NewAuthorizationAsync(new AcmeIdentifier {
				Type = "dns",
				Value = domainName
			});
		}

		public async Task<AcmeAuthorization> GetAuthorizationAsync(Uri location) {
			var data = await _context.SendPostAsGetAsync<AcmeAuthorization>(location).ConfigureAwait(false);
			data.Location = location;
			return data;
		}

		///<summary>
		///<para>Deactivates authorization.</para>
		///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
		///</summary>
		public Task DeactivateAuthorizationAsync(Uri authorizationUri) {
			return _context.SendPostVoidAsync(authorizationUri, new {
				status = AcmeAuthorizationStatus.Deactivated.ToString().ToLower()
			});
		}

		///<summary>
		///<para>Deletes authorization.</para>
		///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-02</para>
		///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-03. Use <see cref="M:DeactivateAuthorizationAsync" /></para>
		///</summary>
		public Task DeleteAuthorizationAsync(Uri authorizationUri) {
			return _context.SendPostVoidAsync(authorizationUri, new {
				resource = "authz",
				delete = true
			});
		}

		public Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallenge challenge) {
			return CompleteChallengeAsync(challenge.Data);
		}

		public Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallengeData challenge) {
			return _context.SendPostKidAsync<object, AcmeChallengeData>(new Uri(challenge.Uri), new {
				type = challenge.Type,
				keyAuthorization = challenge.GetKeyAuthorization(_context.Registration)
			});
		}

	}
}