using System;
using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        public static async Task AcceptToSAsync(CommandLineOptions options) {
            await RequireRegistrationAsync(options);
            var directory = await _client.GetDirectoryAsync();
            var termsOfUse = directory?.Meta?.TermsOfService;
            if (termsOfUse != null) {
                Console.WriteLine("Accepting terms of use");
                Console.WriteLine(termsOfUse);
                await _client.AcceptRegistrationAgreementAsync(termsOfUse);
            }
        }
    }
}