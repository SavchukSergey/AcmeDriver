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
            if (string.IsNullOrWhiteSpace(options.KeyAlgorithm)) {
                throw new CLIException("--key-algorithm is required");
            }
            var parts = options.KeyAlgorithm.Split(':');
            var pem = await (parts[0] switch {
                "rsa" => GenerateRsaPrivateKey(options, parts),
                "ec" => GenerateECPrivateKey(options, parts),
                _ => throw new CLIException($"Unknown private key algorithm {options.KeyAlgorithm}")
            });
            using var writer = new StreamWriter(options.PrivateKeyFile);
            await writer.WriteAsync(pem);
            await writer.FlushAsync();
        }

        private static Task<string> GenerateRsaPrivateKey(CommandLineOptions options, string[] args) {
            if (args.Length != 2) {
                throw new CLIException("Expected format rsa:xxxx");
            }
            if (!int.TryParse(args[1], out var length)) {
                throw new CLIException("Expected format rsa:xxxx");
            }
            var cryptoServiceProvider = new RSACryptoServiceProvider(length);
            var data = cryptoServiceProvider.ExportRSAPrivateKey();
            return Task.FromResult(PemUtils.DERtoPEM(data, "RSA PRIVATE KEY"));
        }

        private static Task<string> GenerateECPrivateKey(CommandLineOptions options, string[] args) {
            if (args.Length != 2) {
                throw new CLIException("Expected format ec:xxxx");
            }
            var curve = ECUtils.GetCurve(args[1]);
            var ecdsa = ECDsa.Create(curve);
            var data = ecdsa.ExportECPrivateKey();
            return Task.FromResult(PemUtils.DERtoPEM(data, "EC PRIVATE KEY"));
        }

    }
}