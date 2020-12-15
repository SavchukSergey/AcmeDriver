using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

        private static async Task CompleteHttpAuthzFilesAsync(CommandLineOptions options) {
            var order = await RequireOrderAsync(options);

            foreach (var authUri in order.Authorizations) {
                var authz = await _client.GetAuthorizationAsync(authUri);
                if (authz.Status == AcmeAuthorizationStatus.Pending) {
                    var httpChallenge = authz.GetHttp01Challenge(_client.Registration);
                    if (httpChallenge != null) {
                        if (await httpChallenge.PrevalidateAsync()) {
                            await _client.CompleteChallengeAsync(httpChallenge);
                        }
                    }
                }
            }
        }

	}
}