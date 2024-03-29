using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeAuthorizationsClient : IAcmeAuthorizationsClient {

		private readonly AcmeAuthenticatedClientContext _context;

		public AcmeAuthorizationsClient(AcmeAuthenticatedClientContext context) {
			_context = context;
		}

		public async Task<AcmeAuthorization> NewAuthorizationAsync(AcmeIdentifier identifier) {
			if (_context.Directory.NewAuthzUrl == null) {
				throw new NotSupportedException("New authorization endpoint is not supported");
			}
			var data = await _context.SendPostAsync<object, AcmeAuthorizationData>(_context.Directory.NewAuthzUrl, new {
				resource = "new-authz",
				identifier = new {
					type = identifier.Type,
					value = identifier.Value
				}
			}).ConfigureAwait(false);
			return new AcmeAuthorization(data, _context.Registration);
		}

		public Task<AcmeAuthorization> NewAuthorizationAsync(string domainName) {
			return NewAuthorizationAsync(new AcmeIdentifier {
				Type = "dns",
				Value = domainName
			});
		}

		public async Task<AcmeAuthorization> GetAuthorizationAsync(Uri location) {
			var data = await _context.SendPostAsGetAsync<AcmeAuthorizationData>(location).ConfigureAwait(false);
			data.Location = location;
			return new AcmeAuthorization(data, _context.Registration);
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

		public async Task<AcmeChallenge> CompleteChallengeAsync(AcmeChallenge challenge) {
			var data = challenge.Data;
			var res = await _context.SendPostKidAsync<object, AcmeChallengeData>(data.Url, new {
				type = data.Type,
				keyAuthorization = data.GetKeyAuthorization(_context.Registration)
			});
			return AcmeChallenge.From(res, challenge.Authorization, _context.Registration);
		}

	}
}