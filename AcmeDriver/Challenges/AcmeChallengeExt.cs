using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AcmeDriver {
    public static class AcmeChallengeExt {

        public static AcmeChallengeData GetHttp01Challenge(this IEnumerable<AcmeChallengeData> challenges) {
            return challenges.FirstOrDefault(ch => ch.Type == "http-01");
        }

        public static AcmeChallengeData GetDns01Challenge(this IEnumerable<AcmeChallengeData> challenges) {
            return challenges.FirstOrDefault(ch => ch.Type == "dns-01");
        }

        public static AcmeHttp01Challenge GetHttp01Challenge(this AcmeAuthorization authorization, AcmeClientRegistration registration) {
            var challenge = authorization.Challenges.GetHttp01Challenge();
            if (challenge == null) return null;
            return new AcmeHttp01Challenge {
                Domain = authorization.Identifier.Value,
                Data = challenge,
                FileName = challenge.Token,
                FileContent = challenge.GetKeyAuthorization(registration)
            };
        }

        public static AcmeDns01Challenge GetDns01Challenge(this AcmeAuthorization authorization, AcmeClientRegistration registration) {
            var challenge = authorization.Challenges.GetDns01Challenge();
            if (challenge == null) return null;
            var keyAuthorization = challenge.GetKeyAuthorization(registration);
            using (var sha256 = SHA256.Create()) {
                return new AcmeDns01Challenge {
                    Domain = authorization.Identifier.Value,
                    Data = challenge,
                    DnsRecordContent = $"{Base64Url.Encode(sha256.ComputeHash(Encoding.UTF8.GetBytes(keyAuthorization)))}"
                };
            }
        }

    }
}
