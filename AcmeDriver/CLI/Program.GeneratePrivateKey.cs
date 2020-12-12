using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        private static async Task GeneratePrivateKeyAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.PrivateKeyFile)) {
                throw new CLIException("--private-key is required");
            }
            var cryptoServiceProvider = new RSACryptoServiceProvider(4096);
            var data = cryptoServiceProvider.ExportRSAPrivateKey();
            var pem = PemUtils.DERtoPEM(data, "PRIVATE KEY");
            using var writer = new StreamWriter(options.PrivateKeyFile);
            await writer.WriteAsync(pem);
            await writer.FlushAsync();
        }

    }
}