using System;
using System.Linq;
using System.Threading.Tasks;
using AcmeDriver.Utils;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static IAcmeClient _client = new AcmeClient(AcmeClient.LETS_ENCRYPT_PRODUCTION_URL);

		public static async Task Main(string[] args) {
			var options = CommandLineOptions.Parse(args);
			Console.WriteLine("AcmeDriver... ready");

			try {
				switch (options.Action) {
					case "ensure-reg":
						await EnsureRegistrationAsync(options);
						break;
					case "new-reg":
						await NewRegistrationAsync(options);
						break;
					case "dump-reg":
						var dumpReg = (await GetClientAsync(options)).Registration;
						ShowRegistrationInfo(dumpReg);
						break;
					case "dump-order":
						await DumpOrderAsync(options);
						break;
					case "accept-tos":
						await AcceptToSAsync(options);
						break;
					case "new-order":
						await NewOrderAsync(options);
						break;
					case "create-http-authz-files":
						await CreateHttpAuthzFilesAsunc(options);
						break;
					case "complete-http-authz-files":
						await CompleteHttpAuthzFilesAsync(options);
						break;
					case "validate-authz-status":
						await ValidateAuthzStatusAsync(options);
						break;
					case "generate-private-key":
						await GeneratePrivateKeyAsync(options);
						break;
					case "ensure-private-key":
						await EnsurePrivateKeyAsync(options);
						break;
					case "generate-csr":
						await GenerateCSRAsync(options);
						break;
					case "run":
						await RunAsync(options);
						break;
					case "finalize-order":
						await FinalizeOrderAsync(options);
						break;
					case "help":
						await ShowHelpAsync(options);
						break;
					case "exit":
						return;
					default:
						Console.WriteLine("unknown command");
						Console.WriteLine("type help to see help screen");
						break;
				}
			} catch (Exception exc) {
				WriteException(exc);
			}
		}

		private static void ShowRegistrationInfo(AcmeClientRegistration reg) {
			Console.WriteLine($"Location: {reg.Location}");
			Console.WriteLine($"JWK:      {reg.GetJwkThumbprint()}");
		}

		private static async Task ShowOrderInfoAsync(AcmeOrder order, CommandLineOptions options) {
			Console.WriteLine($"Location: {order.Location}");
			Console.WriteLine($"Status:        {order.Status}");
			Console.WriteLine($"Expires:       {order.Expires:dd MMM yyy}");
			Console.WriteLine($"Identifiers:   {string.Join(", ", order.Identifiers.Select(item => item.ToString()).ToArray())}");

			Console.WriteLine();
			Console.WriteLine("Authorizations:");
			var client = await GetClientAsync(options);
			foreach (var authUri in order.Authorizations) {
				var authz = await client.Authorizations.GetAuthorizationAsync(authUri);
				await ShowChallengeInfoAsync(authz, options);
			}
		}

		private static async Task ShowChallengeInfoAsync(AcmeAuthorization authz, CommandLineOptions options) {
			var client = await GetClientAsync(options);
			Console.WriteLine($"Authorization: {authz.Identifier.Value}");
			Console.WriteLine($"Status:        {authz.Status}");
			Console.WriteLine($"Expires:       {authz.Expires:dd MMM yyy}");
			Console.WriteLine($"Wildcard:      {authz.Wildcard}");

			Console.WriteLine();
			Console.WriteLine("Challenges:");
			var httpChallenge = authz.GetHttp01Challenge(client.Registration);
			if (httpChallenge != null) {
				ShowChallengeInfo(httpChallenge);
			}

			var dnsChallenge = authz.GetDns01Challenge(client.Registration);
			if (dnsChallenge != null) {
				ShowChallengeInfo(dnsChallenge);
			}
		}

		private static void ShowChallengeInfo(AcmeHttp01Challenge httpChallenge) {
			Console.WriteLine("http-01");
			Console.WriteLine($"FileName:      {httpChallenge.FileName}");
			Console.WriteLine($"FileDirectory: {httpChallenge.FileDirectory}");
			Console.WriteLine($"FileContent:   {httpChallenge.FileContent}");
			Console.WriteLine($"FileUri:       {httpChallenge.FileUri}");
			Console.WriteLine($"---------------");
			Console.WriteLine($"uri:           {httpChallenge.Data.Uri}");
			Console.WriteLine();
		}

		private static void ShowChallengeInfo(AcmeDns01Challenge dnsChallenge) {
			Console.WriteLine("dns-01");
			Console.WriteLine($"DnsRecord:        {dnsChallenge.DnsRecord}");
			Console.WriteLine($"DnsRecordType:    TXT");
			Console.WriteLine($"DnsRecordContent: {dnsChallenge.DnsRecordContent}");
			Console.WriteLine($"---------------");
			Console.WriteLine($"uri:              {dnsChallenge.Data.Uri}");
			Console.WriteLine($"nslookup:         {dnsChallenge.NslookupCmd}");
			Console.WriteLine($"google dns:       {dnsChallenge.GoogleUiApiUrl}");
			Console.WriteLine();
		}

		#region Load & Saving

		private static T Deserialize<T>(string content) {
			return AcmeJson.Deserialize<T>(content);
		}

		private static string Serialize(AcmeOrderModel order) {
			return AcmeJson.Serialize(order);
		}

		#endregion

		private static void WriteErrorLine(string message) {
			var backup = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ForegroundColor = backup;
		}

		private static void WriteException(Exception exc) {
			Console.WriteLine(exc.Message);
			// Console.WriteLine(exc.StackTrace);
			if (exc.InnerException != null) {
				WriteException(exc.InnerException);
			}
		}
	}
}

/*

.\AcmeDriver.exe new-reg --contact mailto:savchuk.sergey@gmail.com --account me.json
.\AcmeDriver.exe ensure-reg --contact mailto:savchuk.sergey@gmail.com --account me.json
.\AcmeDriver.exe accept-tos --account me.json
.\AcmeDriver.exe new-order --domain domain.com --order domain.json --account me.json
.\AcmeDriver.exe create-http-authz-files --order domain.json --challenge .  --account me.json
.\AcmeDriver.exe complete-http-authz-files --order domain.json --challenge .  --account me.json
openssl
.\AcmeDriver.exe finalize-order --order domain.json --csr csrpath --account me.json

*/
