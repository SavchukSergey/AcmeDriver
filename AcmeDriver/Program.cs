using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AcmeDriver.JWK;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class Program {

        private static AcmeClient _client = new AcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL);
        private static AcmeAuthorization _authz;
        private static AcmeOrder _order;

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
                                if (reg != null) ShowRegistrationInfo(reg);
                                else Console.WriteLine("Couldn't load registration");
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
                                var reg = await _client.GetRegistrationAsync(_client.Registration.Location);
                                Console.WriteLine(reg.ToString());
                                ShowRegistrationInfo(_client.Registration);
                            } else {
                                Console.WriteLine("No registration is loaded");
                            }
                            break;
                        case "accept-tos":
                            await _client.AcceptRegistrationAgreementAsync("https://letsencrypt.org/documents/LE-SA-v1.2-November-15-2017.pdf");
                            break;
                        case "new-order":
                            if (args.Length < 1) {
                                ShowNewOrderHelp();
                            } else {
                                var now = DateTime.UtcNow;
                                _order = await _client.NewOrderAsync(new AcmeOrder {
                                    Identifiers = args.Select(arg => new AcmeIdentifier { Type = "dns", Value = arg }).ToArray(),
                                });
                                await SaveOrderAsync(_order, $"order_{_order.Identifiers[0].Value}.json");
                                await ShowOrderInfoAsync(_order);
                            }
                            break;
                        case "order":
                            if (RequireOrder()) {
                                _order = await _client.GetOrderAsync(_order.Location);
                                await SaveOrderAsync(_order, $"order_{_order.Identifiers[0].Value}.json");
                                await ShowOrderInfoAsync(_order);
                            }
                            break;
                        case "finalize-order":
                            if (RequireOrder()) {
                                var csr = await GetCsrAsync(args[0]);
                                await _client.FinalizeOrderAsync(_order, csr);
                            }
                            break;
                        case "authzs":
                            if (RequireOrder()) {
                                _order = await _client.GetOrderAsync(_order.Location); //refresh order
                                foreach (var authUri in _order.Authorizations) {
                                    var authz = await _client.GetAuthorizationAsync(new Uri(authUri));
                                    ShowChallengeInfo(authz);
                                }
                            }
                            break;
                        case "select-authz":
                            if (RequireOrder()) {
                                if (args.Length != 1 || !int.TryParse(args[0], out int index)) {
                                    Console.WriteLine("Usage: select-authz [index]");
                                } else if (index >= 0 && index < _order.Authorizations.Length) {
                                    _authz = await _client.GetAuthorizationAsync(new Uri(_order.Authorizations[index]));
                                } else {
                                    Console.WriteLine($"authz[{index}] not found");
                                }
                            }
                            break;
                        case "authz":
                            if (RequireAuthz()) {
                                _authz = await _client.GetAuthorizationAsync(_authz.Location);
                                ShowChallengeInfo(_authz);
                            }
                            break;
                        case "complete-dns-01":
                            if (RequireAuthz()) {
                                var dnsChallenge = _authz.GetDns01Challenge(_client.Registration);
                                var res = await _client.CompleteChallengeAsync(dnsChallenge);
                            }
                            break;
                        case "complete-http-01":
                            if (RequireAuthz()) {
                                var httpChallenge = _authz.GetHttp01Challenge(_client.Registration);
                                var res = await _client.CompleteChallengeAsync(httpChallenge);
                            }
                            break;
                        case "prevalidate-dns-01":
                            if (RequireAuthz()) {
                                var dnsChallengePrevalidate = await _authz.GetDns01Challenge(_client.Registration).PrevalidateAsync();
                                Console.WriteLine($"Status: {dnsChallengePrevalidate}");
                            }
                            break;
                        case "prevalidate-http-01":
                            if (RequireAuthz()) {
                                var httpChallengePrevalidate = await _authz.GetHttp01Challenge(_client.Registration).PrevalidateAsync();
                                Console.WriteLine($"Status: {httpChallengePrevalidate}");
                            }
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
                        case "help":
                            Console.WriteLine("help                       Show this screen");
                            Console.WriteLine("new-reg [contacts]+        New registration");
                            Console.WriteLine("load-reg [filename]        Load registration from file");
                            Console.WriteLine("save-reg [filename]        Save registration to file");
                            Console.WriteLine("reg                        Show registration info");
                            Console.WriteLine("new-order [identifier]+    Request new order");
                            Console.WriteLine("order                      Refresh order & show order info");
                            Console.WriteLine("finalize-order [csr-path]  Finalize order");
                            Console.WriteLine("accept-tos                 Accept terms of use");
                            Console.WriteLine("new-authz [domain]         Request new authorization");
                            Console.WriteLine("load-authz [domain]        Load authorization");
                            Console.WriteLine("authz                      Refresh authz & show authorization info");
                            Console.WriteLine("complete-dns-01            Complete dns-01 challenge");
                            Console.WriteLine("complete-http-01           Complete http-01 challenge");
                            Console.WriteLine("prevalidate-dns-01         Prevalidate dns-01 challenge");
                            Console.WriteLine("prevalidate-http-01        Prevalidate http-01 challenge");
                            Console.WriteLine("exit                       Exit");
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

        private static bool RequireOrder() {
            if (_order == null) {
                Console.WriteLine("create or select an order first");
                return false;
            }
            return true;
        }

        private static bool RequireAuthz() {
            if (_authz == null) {
                Console.WriteLine("select an authorization first");
                return false;
            }
            return true;
        }

        private static void ShowRegistrationInfo(AcmeClientRegistration reg) {
            Console.WriteLine($"Id:       {reg.Id}");
            Console.WriteLine($"Location: {reg.Location}");
            Console.WriteLine($"JWK:      {reg.GetJwkThumbprint()}");
        }

        private static async Task ShowOrderInfoAsync(AcmeOrder order) {
            Console.WriteLine($"Location: {order.Location}");
            Console.WriteLine($"Status:        {order.Status}");
            Console.WriteLine($"Expires:       {order.Expires:dd MMM yyy}");
            Console.WriteLine($"Identifiers:   {string.Join(", ", order.Identifiers.Select(item => item.ToString()).ToArray())}");

            Console.WriteLine();
            Console.WriteLine("Authorizations:");
            foreach (var authUri in order.Authorizations) {
                var authz = await _client.GetAuthorizationAsync(new Uri(authUri));
                ShowChallengeInfo(authz);
            }
        }

        private static void ShowChallengeInfo(AcmeAuthorization authz) {
            Console.WriteLine($"Authorization: {authz.Identifier.Value}");
            Console.WriteLine($"Status:        {authz.Status}");
            Console.WriteLine($"Expires:       {authz.Expires:dd MMM yyy}");
            Console.WriteLine($"Wildcard:      {authz.Wildcard}");

            Console.WriteLine();
            Console.WriteLine("Challenges:");
            var httpChallenge = authz.GetHttp01Challenge(_client.Registration);
            if (httpChallenge != null) {
                ShowChallengeInfo(httpChallenge);
            }

            var dnsChallenge = authz.GetDns01Challenge(_client.Registration);
            if (dnsChallenge != null) {
                ShowChallengeInfo(dnsChallenge);
            }
        }

        private static void ShowChallengeInfo(AcmeHttp01Challenge httpChallenge) {
            Console.WriteLine("http-01");
            Console.WriteLine($"FileName:      {httpChallenge.FileName}");
            Console.WriteLine($"FileDirectory: {httpChallenge.FileDirectory}");
            Console.WriteLine($"FileContent:   {httpChallenge.FileContent}");
            Console.WriteLine($"FileUri:       {httpChallenge.FileUri}");
            Console.WriteLine($"---------------");
            Console.WriteLine($"uri:           {httpChallenge.Data.Uri}");
            Console.WriteLine();
        }

        private static void ShowChallengeInfo(AcmeDns01Challenge dnsChallenge) {
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

        private static async Task SaveOrderAsync(AcmeOrder order, string filename) {
            try {
                var model = Convert(order);
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
                Key = reg.Key,
                Location = reg.Location
            };
        }

        private static AcmeClientRegistration Convert(AcmeRegistrationModel reg) {
            return new AcmeClientRegistration {
                Id = reg.Id,
                Key = reg.Key,
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

        private static AcmeOrderModel Convert(AcmeOrder order) {
            return new AcmeOrderModel {
                Authorizations = order.Authorizations,
                Expires = order.Expires,
                Finalize = order.Finalize,
                Identifiers = order.Identifiers,
                Location = order.Location,
                Status = order.Status,
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
                Token = challenge.Token,
                Uri = challenge.Uri,
                Type = challenge.Type
            };
        }

        private static AcmeChallengeData Convert(AcmeAuthorizationChallengeModel challenge) {
            return new AcmeChallengeData {
                Token = challenge.Token,
                Uri = challenge.Uri,
                Type = challenge.Type
            };
        }

        private static T Deserialize<T>(string content) {
            return JsonConvert.DeserializeObject<T>(content, new PrivateJwkConverter());
        }

        private static string Serialize(AcmeRegistrationModel reg) {
            return JsonConvert.SerializeObject(reg, Formatting.Indented);
        }

        private static string Serialize(AcmeAuthorizationModel auth) {
            return JsonConvert.SerializeObject(auth, Formatting.Indented);
        }

        private static string Serialize(AcmeOrderModel order) {
            return JsonConvert.SerializeObject(order, Formatting.Indented);
        }

        public class AcmeRegistrationModel {

            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("key")]
            public PrivateJsonWebKey Key { get; set; }

            [JsonProperty("location")]
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

            public string Token { get; set; }

            public string Uri { get; set; }

            public string Type { get; set; }

        }


        public class AcmeOrderModel {

            public AcmeOrderStatus Status { get; set; }

            public DateTimeOffset Expires { get; set; }

            public AcmeIdentifier[] Identifiers { get; set; }

            public string[] Authorizations { get; set; }

            public string Finalize { get; set; }

            public Uri Location { get; set; }
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

        private static void ShowNewOrderHelp() {
            Console.WriteLine("new-order requests new order");
            Console.WriteLine("Usage: new-order domain.com");
        }

        private static void ShowLoadAuthzHelp() {
            Console.WriteLine("load-authz loads authrozation data from file");
            Console.WriteLine("Usage: load-authz domain.com");
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
