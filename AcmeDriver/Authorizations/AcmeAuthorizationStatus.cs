using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public enum AcmeAuthorizationStatus {
        Valid,
        Invalid,
        Pending,
        Deactivated
    }
}
