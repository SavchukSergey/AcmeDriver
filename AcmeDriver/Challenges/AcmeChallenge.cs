using System;
using Newtonsoft.Json;

namespace AcmeDriver {
    public abstract class AcmeChallenge2 {

        public string Domain { get; set; }

        public AcmeChallengeData Data { get; set; }

    }
}
