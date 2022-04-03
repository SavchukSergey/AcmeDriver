Donations: ```bitcoin:1JBnxLMNdvLcEcSP1hrGxdfy3kf7RNnmde```

# CLI
```
AcmeDriver run --domain test.com --challenge /var/www/test.com/.well-known/acme-challenge
```

# Usage
```c#
using var client = await AcmeClient.CreateAcmeClientAsync(AcmeClient.LETS_ENCRYPT_STAGING_URL);
var acme = await client.RegisterAsync(new[] { "mailto:acme@domain.com" });
await acme.Registrations.AcceptAgreementAsync();

var order = await acme.Orders.NewOrderAsync(new AcmeOrder {
    Identifiers = new[] {
        new AcmeIdentifier {
            Type = "dns",
            Value = "domain.com"
        }
    }
});
var authzUri = order.Authorizations[0];
var authz = await acme.Authorizations.GetAuthorizationAsync(authzUri);

Console.WriteLine("Do one of the following:");

var httpChallenge = authz.GetHttp01Challenge();
if (httpChallenge != null) {
    Console.WriteLine("Put file on your http server");
    Console.WriteLine($"FileName: {httpChallenge.FileName}");
    Console.WriteLine($"FileDirectory: {httpChallenge.FileDirectory}");
    Console.WriteLine($"FileContent: {httpChallenge.FileContent}");
    Console.WriteLine();
}

var dnsChallenge = authz.GetDns01Challenge();
if (dnsChallenge != null) {
    Console.WriteLine("Create txt record");
    Console.WriteLine($"DnsRecord: {dnsChallenge.DnsRecord}");
    Console.WriteLine($"DnsRecordContent: {dnsChallenge.DnsRecordContent}");
    Console.WriteLine();
}

Console.WriteLine("And press any key");
Console.ReadKey();

var res = await acme.Authorizations.CompleteChallengeAsync(dnsChallenge);

do {
    authz = await acme.Authorizations.GetAuthorizationAsync(authz.Location);
} while (authz.Status == AcmeAuthorizationStatus.Pending);

var csr = await GetCsrAsync();
await acme.Orders.FinalizeOrderAsync(order, csr);

var crt = await acme.Orders.DownloadCertificateAsync(order);
Console.WriteLine(crt);
}

Task<string> GetCsrAsync() {
return Task.FromResult($@"
-----BEGIN NEW CERTIFICATE REQUEST-----
...
-----END NEW CERTIFICATE REQUEST-----
".Trim());
}
```
