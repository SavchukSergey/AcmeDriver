# Usage
```c#
    using (var client = await AcmeClient.CreateAcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL)) {
        await client.NewRegistrationAsync(new[] { "mailto:savchuk.sergey@gmail.com" });
        await client.AcceptRegistrationAgreementAsync(client.Registration.Location, "https://letsencrypt.org/documents/LE-SA-v1.2-November-15-2017.pdf");

        var order = await client.NewOrderAsync(new AcmeOrder {
            Identifiers = new [] {
                new AcmeIdentifier {
                    Type = "dns",
                    Value = "domain.com"
                }
            }
        });
        var authz = order.Authorizations[0];

        Console.WriteLine("Do one of the following:");

        var httpChallenge = authz.GetHttp01Challenge(client.Registration);
        if (httpChallenge != null) {
            Console.WriteLine("Put file on your http server");
            Console.WriteLine($"FileName: {httpChallenge.FileName}");
            Console.WriteLine($"FileDirectory: {httpChallenge.FileDirectory}");
            Console.WriteLine($"FileContent: {httpChallenge.FileContent}");
            Console.WriteLine();
        }

        var dnsChallenge = authz.GetDns01Challenge(client.Registration);
        if (dnsChallenge != null) {
            Console.WriteLine("Create txt record");
            Console.WriteLine($"DnsRecord: {dnsChallenge.DnsRecord}");
            Console.WriteLine($"DnsRecordContent: {dnsChallenge.DnsRecordContent}");
            Console.WriteLine();
        }

        Console.WriteLine("And press any key");
        Console.ReadKey();

        var res = await client.CompleteChallengeAsync(dnsChallenge);

        do {
            authz = await client.GetAuthorizationAsync(authz.Location);
        } while (authz.Status == AcmeAuthorizationStatus.Pending);

        var csr = await GetCsrAsync();
        await client.FinalizeOrderAsync(order, csr);

        var crt = await client.DownloadCertificateAsync(order);
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

# ACME Resources availablility

| Resource    |  v1 |  v2 |  v3 |  v4 |  v5 |  v6 |  v7 |
|-------------|-----|-----|-----|-----|-----|-----|-----|
| new-reg     |  X  |  X  |  X  |  X  |     |     |     |
| new-account |     |     |     |     |  X  |  X  |  X  |
| new-cert    |  X  |  X  |     |     |     |     |     |
| new-app     |     |     |  X  |  X  |     |     |     |
| new-order   |     |     |     |     |  X  |  X  |  X  |
| new-authz   |  X  |  X  |     |  X  |  X  |  X  |  X  |
| revoke-reg  |  X  |     |     |     |     |     |     |
| revoke-cert |  X  |  X  |  X  |  X  |  X  |  X  |  X  |
| key-change  |     |     |  X  |  X  |  X  |  X  |  X  |
| new-nonce   |     |     |     |  X  |  X  |  X  |  X  |
