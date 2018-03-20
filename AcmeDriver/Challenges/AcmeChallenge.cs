using System.Threading.Tasks;

namespace AcmeDriver {
    public abstract class AcmeChallenge {

        public string Domain { get; set; }

        public AcmeChallengeData Data { get; set; }

        public abstract Task<bool> PrevalidateAsync();

    }
}
