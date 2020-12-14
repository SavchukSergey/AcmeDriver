using System.Linq;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {
        private static async Task NewRegistrationAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.AccountFile)) {
                throw new CLIException("--account is required");
            }
            await _client.NewRegistrationAsync(options.Contacts.ToArray());
            ShowRegistrationInfo(_client.Registration);
            await SaveRegistrationAsync(_client.Registration, options.AccountFile);
        }

    }
}