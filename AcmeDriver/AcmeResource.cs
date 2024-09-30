using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public abstract class AcmeResource {

        [JsonIgnore]
        public Uri Location { get; set; } = default!;

    }
}
