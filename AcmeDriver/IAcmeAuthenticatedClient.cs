using System.Threading.Tasks;

namespace AcmeDriver {
	public interface IAcmeAuthenticatedClient {

		IAcmeOrdersClient Orders { get; }

		IAcmeAuthorizationsClient Authorizations { get; }

		IAcmeRegistrationsClient Registrations { get; }

		AcmeClientRegistration Registration { get; }

		Task InvalidateNonceAsync();

	}
}