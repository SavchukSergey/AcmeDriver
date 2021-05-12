using System;
using System.Threading.Tasks;
using AcmeDriver.JWK;

namespace AcmeDriver {
	public interface IAcmeClient : IDisposable {

		Task<AcmeDirectory> GetDirectoryAsync();

		Task<IAcmeAuthenticatedClient> RegisterAsync(string[] contacts, PrivateJsonWebKey? key = null);

		Task<IAcmeAuthenticatedClient> AuthenticateAsync(PrivateJsonWebKey key);

	}
}