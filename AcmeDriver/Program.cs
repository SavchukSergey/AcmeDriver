using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcmeDriver {
    class Program {
        static void Main(string[] args) {
            Task.Run(MainAsync).Wait();
        }

        private static async Task MainAsync() {
            try {
                var client = new AcmeClient(AcmeClient.STAGING_URL);
                var dir = await client.GetDirectoryAsync();

                var reg = await LoadRegistrationAsync();
                if (reg == null) {
                    await client.NewRegistrationAsync(new[] { "mailto:savchuk.sergey@gmail.com" });
                    reg = client.Registration;
                    await SaveRegistrationASync(reg);
                    await client.AcceptAgreementAsync("https://letsencrypt.org/documents/LE-SA-v1.1.1-August-1-2016.pdf");
                } else {
                    client.Registration = reg;
                }


                var authz = await client.NewAuthorizationAsync("yogam.com.ua");

            } catch (Exception exc) {
                Console.WriteLine(exc.Message);
            }

            Console.ReadKey();
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

        private static async Task SaveRegistrationASync(AcmeClientRegistration reg) {
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
                Key = reg.Key.ExportToXml().ToString()
            };
        }

        private static AcmeClientRegistration Convert(AcmeRegistrationModel reg) {
            var rsa = RSA.Create();
            rsa.ImportFromXml(reg.Key);
            return new AcmeClientRegistration {
                Id = reg.Id,
                Key = rsa
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

        }

    }

}
