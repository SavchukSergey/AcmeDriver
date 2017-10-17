using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcmeDriver {
    public abstract class AcmeChallenge {

        public string Domain { get; set; }

        public AcmeChallengeData Data { get; set; }

        public abstract Task<bool> Prevalidate();

    }
}
