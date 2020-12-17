using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task EnsureRegistrationAsync(CommandLineOptions options) {
            try {
                await GetClientAsync(options);
            } catch {
                await NewRegistrationAsync(options);
            }
        }

    }
}