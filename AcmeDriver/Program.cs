using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AcmeDriver {
    public class Program {

        private static AcmeClient _client = new AcmeClient(AcmeClient.LETS_ENCRYPT_PRODUCTION_URL);
        private static AcmeAuthorization _authz;

        public static async Task Main() {
            Console.WriteLine("AcmeDriver... ready");
            await TryLoadDefaultRegistration();
            while (true) {
                try {
                    var line = Console.ReadLine();
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;
                    var cmd = parts[0];
                    var args = parts.Skip(1).ToArray();
                    switch (cmd) {
                        case "new-reg":
                            await _client.NewRegistrationAsync(args);
                            ShowRegistrationInfo(_client.Registration);
                            break;
                        case "load-reg":
                            if (args.Length != 1) {
                                ShowLoadRegHelp();
                            } else {
                                var reg = await LoadRegistrationAsync(args[0]);
                                _client.Registration = reg;
                            }
                            break;
                        case "save-reg":
                            if (args.Length != 1) {
                                ShowSaveRegHelp();
                            } else {
                                await SaveRegistrationAsync(_client.Registration, args[0]);
                            }
                            break;
                        case "reg":
                            if (_client.Registration != null) {
                                ShowRegistrationInfo(_client.Registration);
                            } else {
                                Console.WriteLine("No registration is loaded");
                            }
                            break;
                        case "accept-tos":
                            await _client.AcceptRegistrationAgreementAsync(_client.Registration.Location, "https://letsencrypt.org/documents/LE-SA-v1.2-November-15-2017.pdf");
                            break;
                        case "new-authz":
                            if (args.Length != 1) {
                                ShowNewAuthzHelp();
                            } else {
                                _authz = await _client.NewAuthorizationAsync(args[0]);
                                await SaveAuthorizationAsync(_authz, $"authz_{args[0]}.json");
                            }
                            break;
                        case "load-authz":
                            if (args.Length != 1) {
                                ShowLoadAuthzHelp();
                            } else {
                                _authz = await LoadAuthorizationAsync($"authz_{args[0]}.json");
                            }
                            break;
                        case "refresh-authz":
                            if (_authz != null) {
                                _authz = await _client.GetAuthorizationAsync(_authz.Location);
                                await SaveAuthorizationAsync(_authz, $"authz_{_authz.Identifier.Value}.json");
                                ShowChallengeInfo(_authz);
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "authz":
                            if (_authz != null) {
                                ShowChallengeInfo(_authz);
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "complete-dns-01":
                            if (_authz != null) {
                                var dnsChallenge = _authz.GetDns01Challenge(_client.Registration);
                                var res = await _client.CompleteChallengeAsync(dnsChallenge);
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "complete-http-01":
                            if (_authz != null) {
                                var httpChallenge = _authz.GetHttp01Challenge(_client.Registration);
                                var res = await _client.CompleteChallengeAsync(httpChallenge);
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "prevalidate-dns-01":
                            if (_authz != null) {
                                var dnsChallengePrevalidate = await _authz.GetDns01Challenge(_client.Registration).PrevalidateAsync();
                                Console.WriteLine($"Status: {dnsChallengePrevalidate}");
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "prevalidate-http-01":
                            if (_authz != null) {
                                var httpChallengePrevalidate = await _authz.GetHttp01Challenge(_client.Registration).PrevalidateAsync();
                                Console.WriteLine($"Status: {httpChallengePrevalidate}");
                            } else {
                                Console.WriteLine("No authorization is loaded");
                            }
                            break;
                        case "new-cert":
                            if (args.Length != 1) {
                                ShowNewCertHelp();
                            } else {
                                var csr = await GetCsrAsync(args[0]);
                                var order = await _client.NewCertificateAsync(new AcmeOrder {
                                    Csr = csr,
                                    NotBefore = DateTime.UtcNow,
                                    NotAfter = DateTime.UtcNow.AddMonths(3)
                                });
                                Console.WriteLine($"Certificate is available at {order}");
                            }
                            break;
                        case "help":
                            Console.WriteLine("help                   Show this screen");
                            Console.WriteLine("new-reg [contacts]+    New registration");
                            Console.WriteLine("load-reg [filename]    Load registration from file");
                            Console.WriteLine("save-reg [filename]    Save registration to file");
                            Console.WriteLine("reg                    Show registration info");
                            Console.WriteLine("accept-tos             Accept terms of use");
                            Console.WriteLine("new-authz [domain]     Request new authorization");
                            Console.WriteLine("load-authz [domain]    Load authorization");
                            Console.WriteLine("refresh-authz          Refresh authorization status");
                            Console.WriteLine("authz                  Show authorization info");
                            Console.WriteLine("complete-dns-01        Complete dns-01 challenge");
                            Console.WriteLine("complete-http-01       Complete http-01 challenge");
                            Console.WriteLine("prevalidate-dns-01     Prevalidate dns-01 challenge");
                            Console.WriteLine("prevalidate-http-01    Prevalidate http-01 challenge");
                            Console.WriteLine("new-cert               Request new certificate");
                            Console.WriteLine("exit                   Exit");
                            break;
                        case "exit":
                            return;
                        default:
                            Console.WriteLine("unknown command");
                            Console.WriteLine("type help to see help screen");
                            break;
                    }
                } catch (Exception exc) {
                    WriteErrorLine(exc.Message);
                }
            }
        }

        private static void ShowRegistrationInfo(AcmeClientRegistration reg) {
            Console.WriteLine($"Id:       {reg.Id}");
            Console.WriteLine($"Location: {reg.Location}");
            Console.WriteLine($"JWK:      {reg.GetJwkThumbprint()}");
        }

        private static void ShowChallengeInfo(AcmeAuthorization authz) {
            Console.WriteLine($"Authorization: {authz.Identifier.Value}");
            Console.WriteLine($"Status:        {authz.Status}");
            Console.WriteLine($"Expires:       {authz.Expires:dd MMM yyy}");

            Console.WriteLine();
            Console.WriteLine("Challenges:");
            var httpChallenge = authz.GetHttp01Challenge(_client.Registration);
            if (httpChallenge != null) {
                Console.WriteLine("http-01");
                Console.WriteLine($"FileName:      {httpChallenge.FileName}");
                Console.WriteLine($"FileDirectory: {httpChallenge.FileDirectory}");
                Console.WriteLine($"FileContent:   {httpChallenge.FileContent}");
                Console.WriteLine($"---------------");
                Console.WriteLine($"uri:           {httpChallenge.Data.Uri}");
                Console.WriteLine();
            }

            var dnsChallenge = authz.GetDns01Challenge(_client.Registration);
            if (dnsChallenge != null) {
                Console.WriteLine("dns-01");
                Console.WriteLine($"DnsRecord:        {dnsChallenge.DnsRecord}");
                Console.WriteLine($"DnsRecordType:    TXT");
                Console.WriteLine($"DnsRecordContent: {dnsChallenge.DnsRecordContent}");
                Console.WriteLine($"---------------");
                Console.WriteLine($"uri:              {dnsChallenge.Data.Uri}");
                Console.WriteLine($"nslookup:         {dnsChallenge.NslookupCmd}");
                Console.WriteLine($"google dns:       {dnsChallenge.GoogleUiApiUrl}");
                Console.WriteLine();
            }

        }

        private static async Task<string> GetCsrAsync(string path) {
            try {
                using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(file)) {
                        var content = await reader.ReadToEndAsync();
                        return content;
                    }
                }
            } catch {
                return null;
            }
        }


        #region Load & Saving

        private static async Task TryLoadDefaultRegistration() {
            _client.Registration = await LoadRegistrationAsync("registration.json");
        }

        private static async Task<AcmeClientRegistration> LoadRegistrationAsync(string filename) {
            try {
                using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(file)) {
                        var content = await reader.ReadToEndAsync();
                        var model = Deserialize<AcmeRegistrationModel>(content);
                        return Convert(model);
                    }
                }
            } catch {
                return null;
            }
        }

        private static async Task SaveRegistrationAsync(AcmeClientRegistration reg, string filename) {
            try {
                var model = Convert(reg);
                var content = Serialize(model);
                using (var file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
                    using (var writer = new StreamWriter(file)) {
                        await writer.WriteAsync(content);
                    }
                }
            } catch {
            }
        }

        private static async Task<AcmeAuthorization> LoadAuthorizationAsync(string filename) {
            try {
                using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(file)) {
                        var content = await reader.ReadToEndAsync();
                        var model = Deserialize<AcmeAuthorizationModel>(content);
                        return Convert(model);
                    }
                }
            } catch {
                return null;
            }
        }

        private static async Task SaveAuthorizationAsync(AcmeAuthorization authz, string filename) {
            try {
                var model = Convert(authz);
                var content = Serialize(model);
                using (var file = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
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

        private static AcmeAuthorizationModel Convert(AcmeAuthorization auth) {
            return new AcmeAuthorizationModel {
                Location = auth.Location,
                Identifier = auth.Identifier,
                Status = auth.Status,
                Expires = auth.Expires,
                Challenges = auth.Challenges.Select(c => Convert(c)).ToArray()
            };
        }

        private static AcmeAuthorization Convert(AcmeAuthorizationModel auth) {
            return new AcmeAuthorization {
                Location = auth.Location,
                Identifier = auth.Identifier,
                Status = auth.Status,
                Expires = auth.Expires,
                Challenges = auth.Challenges.Select(c => Convert(c)).ToArray()
            };
        }

        private static AcmeAuthorizationChallengeModel Convert(AcmeChallengeData challenge) {
            return new AcmeAuthorizationChallengeModel {
                KeyAuthorization = challenge.KeyAuthorization,
                Token = challenge.Token,
                Uri = challenge.Uri,
                Type = challenge.Type
            };
        }

        private static AcmeChallengeData Convert(AcmeAuthorizationChallengeModel challenge) {
            return new AcmeChallengeData {
                KeyAuthorization = challenge.KeyAuthorization,
                Token = challenge.Token,
                Uri = challenge.Uri,
                Type = challenge.Type
            };
        }

        private static T Deserialize<T>(string content) {
            return JsonConvert.DeserializeObject<T>(content);
        }

        private static string Serialize(AcmeRegistrationModel reg) {
            return JsonConvert.SerializeObject(reg, Formatting.Indented);
        }

        private static string Serialize(AcmeAuthorizationModel auth) {
            return JsonConvert.SerializeObject(auth, Formatting.Indented);
        }

        public class AcmeRegistrationModel {

            public long Id { get; set; }

            public string Key { get; set; }

            public Uri Location { get; set; }

        }

        public class AcmeAuthorizationModel {

            public Uri Location { get; set; }

            public AcmeAuthorizationChallengeModel[] Challenges { get; set; }

            public AcmeIdentifier Identifier { get; set; }

            public DateTimeOffset Expires { get; set; }

            public AcmeAuthorizationStatus Status { get; set; }

        }

        public class AcmeAuthorizationChallengeModel {

            public string KeyAuthorization { get; set; }

            public string Token { get; set; }

            public string Uri { get; set; }

            public string Type { get; set; }

        }

        #endregion

        #region Help

        private static void ShowLoadRegHelp() {
            Console.WriteLine("load-reg loads registration data from file");
            Console.WriteLine("Usage: load-reg filename.json");
        }

        private static void ShowSaveRegHelp() {
            Console.WriteLine("save-reg saves registration data to file");
            Console.WriteLine("Usage: save-reg filename.json");
        }

        private static void ShowNewAuthzHelp() {
            Console.WriteLine("new-authz requests new authorization");
            Console.WriteLine("Usage: new-authz domain.com");
        }

        private static void ShowLoadAuthzHelp() {
            Console.WriteLine("load-authz loads authrozation data from file");
            Console.WriteLine("Usage: load-authz domain.com");
        }

        private static void ShowNewCertHelp() {
            Console.WriteLine("new-cert requests new certificate");
            Console.WriteLine("Usage: new-cert pem-encoded-file.csr");
        }

        #endregion

        private static void WriteErrorLine(string message) {
            var backup = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = backup;
        }
    }
}
