using System;
using AcmeDriver.JWK;

namespace AcmeDriver {
    public class AcmeClientRegistration {

        public PrivateJsonWebKey Key { get; set; }

        public Uri Location { get; set; }

        public string GetJwkThumbprint() {
            return Key.GetPublicJwk().GetJwkThumbprint();
        }

    }
}
