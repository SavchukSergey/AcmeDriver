using System;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static Task ShowHelpAsync(CommandLineOptions options) {
			Console.WriteLine("Usage:");
			Console.WriteLine("AcmeDriver action [--arg value]");
            Console.WriteLine();
			Console.WriteLine("help                         Show action help screen");
			Console.WriteLine("run                          Run full certificate workflow");
			Console.WriteLine("ensure-reg                   Ensure registration");
			Console.WriteLine("new-reg                      New registration");
			Console.WriteLine("dump-reg                     Dump registration");
			Console.WriteLine("accept-tos                   Accept terms of use");
			Console.WriteLine("new-order                    Request new order");
			Console.WriteLine("create-http-authz-files      Create http-challenge file");
			Console.WriteLine("complete-http-authz-files    Complete http-challanges");
			Console.WriteLine("validate-authz-status        Validate order authorizations status");
			Console.WriteLine("generate-private-key         Generate private key");
			Console.WriteLine("ensure-private-key           Ensure private key");
			Console.WriteLine("generate-csr                 Generate certificate request file");
			Console.WriteLine("finalize-order               Finalize order");
			Console.WriteLine("exit                         Exit");
            return Task.CompletedTask;
		}

		private static Task ShowNewOrderHelpAsync(CommandLineOptions options) {
			Console.WriteLine("Usage of new-order action:");
			Console.WriteLine("new-order --domain test.com --domain www.test.com");
            return Task.CompletedTask;
		}

	}
}