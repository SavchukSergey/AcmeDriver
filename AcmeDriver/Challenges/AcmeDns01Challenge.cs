using System;

namespace AcmeDriver {

    public class AcmeDns01Challenge : AcmeChallenge {

        public string DnsRecord => "_acme-challenge";

        public string DnsAddress => $"{DnsRecord}.{Domain}";

        public string DnsRecordContent { get; set; }

        public string NslookupCmd => $"nslookup -type=TXT {DnsAddress}";

    }

}