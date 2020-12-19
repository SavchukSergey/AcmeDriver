using System;
using System.Linq;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static async Task NewOrderAsync(CommandLineOptions options) {
			if (options.Domains.Count == 0) {
				await ShowNewOrderHelpAsync(options);
			} else {
				var now = DateTime.UtcNow;
				var client = await GetClientAsync(options);
				var order = await client.Orders.NewOrderAsync(new AcmeOrder {
					Identifiers = options.Domains.Select(arg => new AcmeIdentifier { Type = "dns", Value = arg }).ToArray(),
				});
				await SaveOrderAsync(options, order);
				// await ShowOrderInfoAsync(order);
			}
		}

	}
}