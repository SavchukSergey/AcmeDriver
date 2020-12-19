using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AcmeDriver.Utils;

namespace AcmeDriver.CLI {
	public partial class Program {

		private static async Task<AcmeAuthenticatedClient> GetClientAsync(CommandLineOptions options) {
			try {
				var key = await LoadAccountPrivateKeyAsync(options);
				var jwk = PrivateKeyUtils.ToPrivateJsonWebKey(key);
				return await _client.AuthenticateAsync(jwk);
			} catch (Exception exc) {
				throw new CLIException("Unable to read ACCOUNT file", exc);
			}
		}

		private static async Task<AcmeOrder> RequireOrderAsync(CommandLineOptions options) {
			if (string.IsNullOrWhiteSpace(options.OrderFile)) {
				throw new CLIException("--order is required");
			}
			try {
				using var file = File.Open(options.OrderFile, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new StreamReader(file);
				var content = await reader.ReadToEndAsync();
				var model = Deserialize<AcmeOrderModel>(content);

				var client = await GetClientAsync(options);
				return await client.Orders.GetOrderAsync(model.Location);
			} catch (Exception exc) {
				throw new CLIException("Unable to read ORDER file", exc);
			}
		}

		private static async Task SaveOrderAsync(CommandLineOptions options, AcmeOrder order) {
			if (string.IsNullOrWhiteSpace(options.OrderFile)) {
				throw new CLIException("--order is required");
			}
			try {
				var model = AcmeOrderModel.From(order);
				var content = Serialize(model);
				using var file = File.Open(options.OrderFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
				using var writer = new StreamWriter(file);
				await writer.WriteAsync(content);
			} catch (Exception exc) {
				throw new CLIException("Unable to save order", exc);
			}
		}

		private static async Task<string> RequireCsrAsync(CommandLineOptions options) {
			if (string.IsNullOrWhiteSpace(options.CsrFile)) {
				throw new CLIException("--csr is required");
			}
			try {
				using var file = File.Open(options.CsrFile, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new StreamReader(file);
				var content = await reader.ReadToEndAsync();
				return content;
			} catch (Exception exc) {
				throw new CLIException("Unable to read CSR file", exc);
			}
		}

		private static Task<AsymmetricAlgorithm> RequirePrivateKeyAsync(CommandLineOptions options) {
			if (string.IsNullOrWhiteSpace(options.PrivateKeyFile)) {
				throw new CLIException("--private-key is required");
			}
			return LoadPrivateKeyAsync(options.PrivateKeyFile);
		}

		private static Task<AsymmetricAlgorithm> LoadAccountPrivateKeyAsync(CommandLineOptions options) {
			if (string.IsNullOrWhiteSpace(options.AccountFile)) {
				throw new CLIException("--account is required");
			}
			return LoadPrivateKeyAsync(options.AccountFile);
		}

		private static async Task<AsymmetricAlgorithm> LoadPrivateKeyAsync(string filePath) {
			try {
				using var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new StreamReader(file);
				var keyPEM = await reader.ReadToEndAsync();
				return PrivateKeyUtils.ToAsymmetricAlgorithm(keyPEM);
			} catch (Exception exc) {
				throw new CLIException("Unable to read Private Key file", exc);
			}
		}

		private static async Task SaveRegistrationAsync(AcmeClientRegistration reg, string filename) {
			try {
				var content = PrivateKeyUtils.ToPem(reg.Key);
				using var file = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
				using var writer = new StreamWriter(file);
				await writer.WriteAsync(content);
				await writer.FlushAsync();
			} catch (Exception exc) {
				throw new CLIException("Unable to save account", exc);
			}
		}

	}
}