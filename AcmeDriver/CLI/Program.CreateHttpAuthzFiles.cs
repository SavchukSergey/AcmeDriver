using System.IO;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static async Task CreateHttpAuthzFilesAsunc(CommandLineOptions options) {
			if (string.IsNullOrWhiteSpace(options.ChallengePath)) {
				throw new CLIException("--challenge is required");
			}
			Directory.CreateDirectory(options.ChallengePath);
			var order = await RequireOrderAsync(options);
			var client = await GetClientAsync(options);
			foreach (var authUri in order.Authorizations) {
				var authz = await client.Authorizations.GetAuthorizationAsync(authUri);
				var httpChallenge = authz.GetHttp01Challenge(client.Registration);
				if (httpChallenge != null) {
					var path = Path.Combine(options.ChallengePath, httpChallenge.FileName);
					using var writer = new StreamWriter(path);
					await writer.WriteAsync(httpChallenge.FileContent);
					await writer.FlushAsync();
				}
			}
		}

	}
}