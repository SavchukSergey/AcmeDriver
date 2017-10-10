using System;
using System.Collections.Generic;
using System.Linq;

namespace AcmeDriver {
    public static class AcmeChallengeExt {

        public static AcmeChallenge GetHttp01Challenge(this IEnumerable<AcmeChallenge> challenges) {
            return challenges.FirstOrDefault(ch => ch.Type == "http-01");
        }

        public static AcmeChallenge GetDns01Challenge(this IEnumerable<AcmeChallenge> challenges) {
            return challenges.FirstOrDefault(ch => ch.Type == "dns-01");
        }

    }
}
