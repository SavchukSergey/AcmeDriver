using System.IO;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task FinalizeOrderAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.CrtFile)) {
                throw new CLIException("--crt is required");
            }
            var order = await RequireOrderAsync(options);
            var csr = await RequireCsrAsync(options);
            var client = await GetClientAsync(options);
            var newOrder = await client.Orders.FinalizeOrderAsync(order, csr);
            await SaveOrderAsync(options, newOrder);
            var cert = await client.Orders.DownloadCertificateAsync(newOrder);
            var writer = new StreamWriter(options.CrtFile);
            await writer.WriteAsync(cert);
            await writer.FlushAsync();
        }

    }
}