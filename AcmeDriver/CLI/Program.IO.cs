using System;
using System.IO;
using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        private static async Task<AcmeClientRegistration> RequireRegistrationAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.AccountFile)) {
                throw new CLIException("--account is required");
            }
            try {
                using var file = File.Open(options.AccountFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(file);
                var content = await reader.ReadToEndAsync();
                var model = Deserialize<AcmeRegistrationModel>(content);
                var res = Convert(model);
                _client.Registration = res;
                return res;
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

                await RequireRegistrationAsync(options);
                return await _client.GetOrderAsync(model.Location);
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

    }
}