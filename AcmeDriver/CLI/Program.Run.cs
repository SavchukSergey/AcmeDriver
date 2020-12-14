using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task RunAsync(CommandLineOptions options) {
            if (options.Domains.Count == 0) {
                throw new CLIException("--domain is required");
            }
            var primaryDomain = options.Domains[0];
            var domainsLabel = string.Join("_", options.Domains);
            if (string.IsNullOrWhiteSpace(options.AccountFile)) {
                options.AccountFile = "lets_encrypt_account.key";
            }
            if (string.IsNullOrWhiteSpace(options.OrderFile)) {
                options.OrderFile = $"{domainsLabel}.order.json";
            }
            if (string.IsNullOrWhiteSpace(options.PrivateKeyFile)) {
                options.PrivateKeyFile = $"{domainsLabel}.key";
            }
            if (string.IsNullOrWhiteSpace(options.KeyAlgorithm)) {
                options.KeyAlgorithm = "ec:P-384";
            }
            if (string.IsNullOrWhiteSpace(options.CsrFile)) {
                options.CsrFile = $"{domainsLabel}.csr";
            }
            if (string.IsNullOrWhiteSpace(options.CrtFile)) {
                options.CrtFile = $"{domainsLabel}.crt";
            }
            if (string.IsNullOrWhiteSpace(options.ChallengePath)) {
                options.ChallengePath = ".";
            }
            if (string.IsNullOrWhiteSpace(options.Subject)) {
                options.Subject = $"CN={primaryDomain}";
            }
            if (options.Contacts.Count == 0) {
                options.Contacts.Add($"mailto:noreply@{primaryDomain}");
            }
            await EnsureRegistrationAsync(options);
            await AcceptToSAsync(options);
            await NewOrderAsync(options);
            await CreateHttpAuthzFiles(options);
            await CompleteHttpAuthzFiles(options);
            await ValidateAuthzStatusAsync(options);
            await GeneratePrivateKeyAsync(options);
            await GenerateCSRAsync(options);
            await FinalizeOrderAsync(options);
        }

    }
}