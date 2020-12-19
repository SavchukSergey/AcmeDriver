using System;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		public static async Task AcceptToSAsync(CommandLineOptions options) {
			var client = await GetClientAsync(options);
			var directory = client.Directory;
			Console.WriteLine("Accepting terms of use");
			await client.Registrations.AcceptAgreementAsync();
		}
	}
}