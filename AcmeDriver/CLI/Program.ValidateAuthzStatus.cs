using System;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task ValidateAuthzStatusAsync(CommandLineOptions options) {
            var order = await RequireOrderAsync(options);
            var client = await GetClientAsync(options);
            for (var i = 0; i < 10; i++) {
                bool good = true;
                foreach (var authUri in order.Authorizations) {
                    var authz = await client.Authorizations.GetAuthorizationAsync(authUri);
                    if (authz.Status != AcmeAuthorizationStatus.Valid) {
                        good = false;
                        break;
                    }
                }
                if (!good) {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                } else {
                    return;
                }
            }
            foreach (var authUri in order.Authorizations) {
                var authz = await client.Authorizations.GetAuthorizationAsync(authUri);
                if (authz.Status != AcmeAuthorizationStatus.Valid) {
                    throw new CLIException($"Authorization for domain {authz.Identifier} has {authz.Status} status but {AcmeAuthorizationStatus.Valid} is required");
                }
            }
        }

    }
}