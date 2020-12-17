using System.Linq;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {
        
        private static async Task NewRegistrationAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.AccountFile)) {
                throw new CLIException("--account is required");
            }
            var client = await _client.RegisterAsync(options.Contacts.ToArray());
            ShowRegistrationInfo(client.Registration);
            await SaveRegistrationAsync(client.Registration, options.AccountFile);
        }

    }
}