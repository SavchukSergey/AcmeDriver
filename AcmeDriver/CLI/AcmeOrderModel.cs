using System;

namespace AcmeDriver.CLI {
    public class AcmeOrderModel {

        public AcmeOrderStatus Status { get; set; }

        public DateTimeOffset Expires { get; set; }

        public AcmeIdentifier[] Identifiers { get; set; }

        public string[] Authorizations { get; set; }

        public string Finalize { get; set; }

        public Uri Location { get; set; }
    }

}