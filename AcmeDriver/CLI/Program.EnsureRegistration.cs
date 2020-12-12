using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        private static async Task EnsureRegistrationAsync(CommandLineOptions options) {
            try {
                await RequireRegistrationAsync(options);
            } catch {
                await NewRegistrationAsync(options);
            }
        }

    }
}