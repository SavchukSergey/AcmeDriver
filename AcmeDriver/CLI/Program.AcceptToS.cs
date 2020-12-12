using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        public static async Task AcceptToSAsync(CommandLineOptions options) {
            await RequireRegistrationAsync(options);
            await _client.AcceptRegistrationAgreementAsync("https://letsencrypt.org/documents/LE-SA-v1.2-November-15-2017.pdf");
        }
    }
}