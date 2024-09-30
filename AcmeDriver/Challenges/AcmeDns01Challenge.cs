using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AcmeDriver.Utils;

namespace AcmeDriver {
    public class AcmeDns01Challenge : AcmeChallenge {

        public string DnsRecord => "_acme-challenge";

        public string DnsAddress => $"{DnsRecord}.{Domain}";

        public string DnsRecordContent { get; }

        public string NslookupCmd => $"nslookup -type=TXT {DnsAddress}";

        public string GoogleApiUrl => $"https://dns.google.com/resolve?name={DnsAddress}&type=TXT";

        public string GoogleUiApiUrl => $"https://dns.google.com/query?name={DnsAddress}&type=TXT";

        public AcmeDns01Challenge(AcmeChallengeData data, AcmeAuthorization authorization, AcmeClientRegistration registration) : base(data, authorization, registration) {
			var keyAuthorization = data.GetKeyAuthorization(registration);
			DnsRecordContent = Base64Url.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(keyAuthorization)));
		}

		public override async Task<bool> PrevalidateAsync() {
            try {
				using var client = new HttpClient();
				var responseContent = await client.GetStringAsync(GoogleApiUrl).ConfigureAwait(false);
				var res = AcmeJson.Deserialize<GoogleDnsApiResponse>(responseContent);
				return res.Answers.Any(a => a.Type == GoogleDnsRecordType.TXT && (a.Data == DnsRecordContent || a.Data == $"\"{DnsRecordContent}\""));
			} catch {
                return false;
            }
        }

        public class GoogleDnsApiResponse {

            [JsonPropertyName("Answer")]
            public IList<GoogleDnsApiResponseAnswer> Answers { get; } = [];

        }

        public class GoogleDnsApiResponseAnswer {

            [JsonPropertyName("name")]
            public string Name { get; set; } = default!;

            [JsonPropertyName("type")]
            public GoogleDnsRecordType Type { get; set; }

            [JsonPropertyName("data")]
            public string Data { get; set; } = default!;

        }

        public enum GoogleDnsRecordType {
            TXT = 16
        }

    }
}