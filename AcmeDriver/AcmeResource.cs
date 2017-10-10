using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public abstract class AcmeResource {

        [JsonIgnore]
        public Uri Location { get; set; }

    }
}
