using System;

namespace AcmeDriver.CLI {
    public class AcmeOrderModel {

        public AcmeOrderStatus Status { get; set; }

        public DateTimeOffset Expires { get; set; }

        public AcmeIdentifier[] Identifiers { get; set; } = default!;

        public Uri[] Authorizations { get; set; } = default!;

        public Uri Finalize { get; set; } = default!;

        public Uri Location { get; init; } = default!;

		public static AcmeOrderModel From(AcmeOrder order) {
			return new AcmeOrderModel {
				Authorizations = order.Authorizations,
				Expires = order.Expires,
				Finalize = order.Finalize,
				Identifiers = order.Identifiers,
				Location = order.Location,
				Status = order.Status,
			};
		}

    }

}