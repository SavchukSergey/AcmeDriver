using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static async Task CompleteHttpAuthzFilesAsync(CommandLineOptions options) {
			var order = await RequireOrderAsync(options);
			Console.WriteLine("Completing challenges");

			var client = await GetClientAsync(options);

			async IAsyncEnumerable<ProgressStatusItem> CheckAuthz(Uri authUri, ProgressStatusItem item) {
				var authz = await client.Authorizations.GetAuthorizationAsync(authUri);
				yield return item.SetSubject(authz.Identifier.Value);
				for (var i = 1; i <= 10; i++) {
					yield return item.SetInfo($"{i}/10");
					if (authz.Status == AcmeAuthorizationStatus.Pending) {
						var httpChallenge = authz.GetHttp01Challenge();
						if (httpChallenge != null) {
							if (await httpChallenge.PrevalidateAsync()) {
								await client.Authorizations.CompleteChallengeAsync(httpChallenge);
								yield return item.SetOk();
								yield break;
							} else {
								yield return item.SetPending();
							}
						} else {
							yield return item.SetSkipped();
							yield break;
						}
					} else if (authz.Status == AcmeAuthorizationStatus.Valid) {
						yield return item.SetOk();
						yield break;
					} else {
						yield return item.SetStatus($"[{authz.Status}]");
						yield break;
					}
					await Task.Delay(TimeSpan.FromSeconds(1));
				}
				yield return item.SetFailed();
			}

			await new ProgressStatus().RunAsync(order.Authorizations, (authUri, item) => CheckAuthz(authUri, item));
		}

	}

}