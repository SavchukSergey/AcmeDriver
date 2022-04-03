using System.Linq;

namespace AcmeDriver {
	public static class AcmeChallengeExt {

        public static AcmeHttp01Challenge? GetHttp01Challenge(this AcmeAuthorization authorization) {
            return authorization.Challenges.OfType<AcmeHttp01Challenge>().FirstOrDefault();
        }

        public static AcmeDns01Challenge? GetDns01Challenge(this AcmeAuthorization authorization) {
            return authorization.Challenges.OfType<AcmeDns01Challenge>().FirstOrDefault();
        }

    }
}
