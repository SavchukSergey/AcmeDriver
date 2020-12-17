using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeRegistrationsClient {

		private readonly AcmeAuthenticatedClientContext _context;

		public AcmeRegistrationsClient(AcmeAuthenticatedClientContext context) {
			_context = context;
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