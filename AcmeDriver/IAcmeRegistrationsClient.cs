using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public interface IAcmeRegistrationsClient {

		Task<AcmeRegistration> AcceptAgreementAsync();

		Task<AcmeRegistration> AcceptAgreementAsync(Uri agreementUrl);

		Task<AcmeRegistration> GetRegistrationAsync();

		Task UpdateRegistrationAsync(Uri registrationUri);

	}
}