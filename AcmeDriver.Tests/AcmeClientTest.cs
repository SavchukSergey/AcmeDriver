using System;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace AcmeDriver.Tests {
    [TestFixture]
    public class AcmeClientTest {

        [Test]
        public async Task WorkflowTest() {
            try {
                var client = await AcmeClient.CreateAcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL);
                // var dir = await client.GetDirectoryAsync();
                var reg = await LoadRegistrationAsync();
                if (reg == null) {
                    await client.NewRegistrationAsync(new[] { "mailto:savchuk.sergey@gmail.com" });
                    reg = client.Registration;
                    await SaveRegistrationAsync(reg);
                    await client.AcceptRegistrationAgreementAsync(reg.Location, "https://letsencrypt.org/documents/LE-SA-v1.1.1-August-1-2016.pdf");
                } else {
                    client.Registration = reg;
                }

                var authz = await client.NewAuthorizationAsync("domain.com");

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
                var order = await client.NewCertificateAsync(new AcmeOrder {
                    Csr = csr,
                    NotBefore = DateTime.UtcNow,
                    NotAfter = DateTime.UtcNow.AddMonths(1)
                });

                var crt = await client.DownloadCertificateAsync(order);
                Console.WriteLine(crt);
            } catch (Exception exc) {
                Console.WriteLine(exc.Message);
            }

            Console.ReadKey();
        }


        private static async Task<string> GetCsrAsync() {
            try {
                using (var file = File.Open("../../../test.csr.txt", FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(file)) {
                        var content = await reader.ReadToEndAsync();
                        return content;
                    }
                }
            } catch {
                return null;
            }
        }

        private static async Task<AcmeClientRegistration> LoadRegistrationAsync() {
            try {
                using (var file = File.Open("reg.xml", FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(file)) {
                        var content = await reader.ReadToEndAsync();
                        var model = Deserialize(content);
                        return Convert(model);
                    }
                }
            } catch {
                return null;
            }
        }

        private static async Task SaveRegistrationAsync(AcmeClientRegistration reg) {
            try {
                var model = Convert(reg);
                var content = Serialize(model);
                using (var file = File.Open("reg.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    using (var writer = new StreamWriter(file)) {
                        await writer.WriteAsync(content);
                    }
                }
            } catch {
            }
        }

        private static AcmeRegistrationModel Convert(AcmeClientRegistration reg) {
            return new AcmeRegistrationModel {
                Id = reg.Id,
                Key = reg.Key.ExportToXml().ToString(),
                Location = reg.Location
            };
        }

        private static AcmeClientRegistration Convert(AcmeRegistrationModel reg) {
            var rsa = RSA.Create();
            rsa.ImportFromXml(reg.Key);
            return new AcmeClientRegistration {
                Id = reg.Id,
                Key = rsa,
                Location = reg.Location
            };
        }

        private static AcmeRegistrationModel Deserialize(string content) {
            return JsonConvert.DeserializeObject<AcmeRegistrationModel>(content);
        }

        private static string Serialize(AcmeRegistrationModel reg) {
            return JsonConvert.SerializeObject(reg);
        }

        public class AcmeRegistrationModel {

            public long Id { get; set; }

            public string Key { get; set; }

            public Uri Location { get; set; }

        }

    }
}