using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcmeDriver {

    public class AcmeDns01Challenge : AcmeChallenge {

        public string DnsRecord => "_acme-challenge";

        public string DnsAddress => $"{DnsRecord}.{Domain}";

        public string DnsRecordContent { get; set; }

        public string NslookupCmd => $"nslookup -type=TXT {DnsAddress}";

        public string GoogleApiUrl => $"https://dns.google.com/resolve?name={DnsAddress}&type=TXT";

        public string GoogleUiApiUrl => $"https://dns.google.com/query?name={DnsAddress}&type=TXT";

        public override async Task<bool> Prevalidate() {
            try {
                using (var client = new HttpClient()) {
                    var responseContent = await client.GetStringAsync(GoogleApiUrl);
                    var res = JsonConvert.DeserializeObject<GoogleDnsApiResponse>(responseContent);
                    return res.Answers.Any(a => a.Type == GoogleDnsRecordType.TXT && a.Data == DnsRecordContent);
                }
            } catch {
                return false;
            }
        }

        public class GoogleDnsApiResponse {

            [JsonProperty("Answer")]
            public IList<GoogleDnsApiResponseAnswer> Answers { get; } = new List<GoogleDnsApiResponseAnswer>();

        }

        public class GoogleDnsApiResponseAnswer {

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public GoogleDnsRecordType Type { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }

        }

        public enum GoogleDnsRecordType {
            TXT = 16
        }

    }
}