using System;

namespace AcmeDriver {
	public class AcmeAuthenticatedClient {

		private readonly AcmeAuthenticatedClientContext _context;

        private readonly Lazy<AcmeOrdersClient> _orders;
        private readonly Lazy<AcmeAuthorizationsClient> _authorizations;
        private readonly Lazy<AcmeRegistrationsClient> _registrations;

        public AcmeOrdersClient Orders => _orders.Value;
        public AcmeAuthorizationsClient Authorizations => _authorizations.Value;
        public AcmeRegistrationsClient Registrations => _registrations.Value;

        public AcmeClientRegistration Registration => _context.Registration;
        public AcmeDirectory Directory => _context.Directory;

		public AcmeAuthenticatedClient(AcmeAuthenticatedClientContext context) {
			_context = context;
            _orders = new Lazy<AcmeOrdersClient>(() => new AcmeOrdersClient(_context));
            _authorizations = new Lazy<AcmeAuthorizationsClient>(() => new AcmeAuthorizationsClient(_context));
            _registrations = new Lazy<AcmeRegistrationsClient>(() => new AcmeRegistrationsClient(_context));
		}

	}
}