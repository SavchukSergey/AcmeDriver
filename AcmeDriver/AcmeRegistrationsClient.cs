using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeRegistrationsClient : IAcmeRegistrationsClient {

		private readonly AcmeAuthenticatedClientContext _context;

		public AcmeRegistrationsClient(AcmeAuthenticatedClientContext context) {
			_context = context;
		}

		public Task<AcmeRegistration> AcceptAgreementAsync() {
			var tosUrl = _context.Directory.Meta?.TermsOfService;
			if (tosUrl != null) {
				return AcceptAgreementAsync(tosUrl);
			}
			return GetRegistrationAsync();
		}

		public Task<AcmeRegistration> AcceptAgreementAsync(Uri agreementUrl) {
			return _context.SendPostKidAsync<object, AcmeRegistration>(_context.Registration.Location, new {
				resource = "reg",
				agreement = agreementUrl
			});
		}

		public Task<AcmeRegistration> GetRegistrationAsync() {
			return _context.SendPostAsGetAsync<AcmeRegistration>(_context.Registration.Location, (headers, reg) => {
				reg.Location = headers.Location ?? _context.Registration.Location;
			});
		}

		public Task UpdateRegistrationAsync(Uri registrationUri) {
			return Task.CompletedTask;
		}

	}
}