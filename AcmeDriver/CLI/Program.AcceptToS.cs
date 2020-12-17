using System;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        public static async Task AcceptToSAsync(CommandLineOptions options) {
            var client = await GetClientAsync(options);
            var directory = client.Directory;
            var termsOfUse = directory?.Meta?.TermsOfService;
            if (termsOfUse != null) {
                Console.WriteLine("Accepting terms of use");
                Console.WriteLine(termsOfUse);
                await client.Registrations.AcceptAgreementAsync(termsOfUse);
            }
        }
    }
}