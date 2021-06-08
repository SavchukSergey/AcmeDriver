using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeAuthenticatedClient : IAcmeAuthenticatedClient {

		private readonly AcmeAuthenticatedClientContext _context;

        private readonly Lazy<IAcmeOrdersClient> _orders;
        private readonly Lazy<IAcmeAuthorizationsClient> _authorizations;
        private readonly Lazy<IAcmeRegistrationsClient> _registrations;

        public IAcmeOrdersClient Orders => _orders.Value;
        public IAcmeAuthorizationsClient Authorizations => _authorizations.Value;
        public IAcmeRegistrationsClient Registrations => _registrations.Value;

        public AcmeClientRegistration Registration => _context.Registration;
        public AcmeDirectory Directory => _context.Directory;

		public AcmeAuthenticatedClient(AcmeAuthenticatedClientContext context) {
			_context = context;
            _orders = new Lazy<IAcmeOrdersClient>(() => new AcmeOrdersClient(_context));
            _authorizations = new Lazy<IAcmeAuthorizationsClient>(() => new AcmeAuthorizationsClient(_context));
            _registrations = new Lazy<IAcmeRegistrationsClient>(() => new AcmeRegistrationsClient(_context));
		}

	}
}