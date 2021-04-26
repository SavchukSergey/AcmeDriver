using System;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static async Task DumpOrderAsync(CommandLineOptions options) {
			if (options.OrderUrl == null) {
				throw new CLIException("--order-url is required");
			}
			var client = await GetClientAsync(options);
            var order = await client.Orders.GetOrderAsync(options.OrderUrl);
            Console.WriteLine($"Status: {order.Status}");
            Console.WriteLine($"Expires: {order.Expires}");
		}

	}
}