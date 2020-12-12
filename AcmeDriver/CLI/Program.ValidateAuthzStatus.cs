using System;
using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        private static async Task ValidateAuthzStatusAsync(CommandLineOptions options) {
            var order = await RequireOrderAsync(options);
            foreach (var authUri in order.Authorizations) {
                var authz = await _client.GetAuthorizationAsync(new Uri(authUri));
                if (authz.Status != AcmeAuthorizationStatus.Valid) {
                    throw new CLIException($"Authorization for domain {authz.Identifier} has {authz.Status} status but {AcmeAuthorizationStatus.Valid} is required");
                }
            }
        }

    }
}