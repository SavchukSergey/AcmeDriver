using System;

namespace AcmeDriver {

    public class AcmeDns01Challenge : AcmeChallenge2 {

        public string DnsRecord => "_acme-challenge";

        public string DnsAddress => $"{DnsRecord}.{Domain}";

        public string DnsRecordContent { get; set; }

    }

}