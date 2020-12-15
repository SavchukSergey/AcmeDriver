using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task EnsurePrivateKeyAsync(CommandLineOptions options) {
            try {
                await RequirePrivateKeyAsync(options);
            } catch {
                await GeneratePrivateKeyAsync(options);
            }
        }

    }
}