using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AcmeDriver.CLI;

namespace AcmeDriver {
    public partial class Program {

        private static async Task<string> GenerateCSRAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.CsrFile)) {
                throw new CLIException("--csr is required");
            }
            if (string.IsNullOrWhiteSpace(options.PrivateKeyFile)) {
                throw new CLIException("--private-key is required");
            }
            if (string.IsNullOrWhiteSpace(options.Subject)) {
                throw new CLIException("--subject is required");
            }

            var subjectName = options.Subject;

            var keyReader = new StreamReader(options.PrivateKeyFile);
            var keyPEM = await keyReader.ReadToEndAsync();
            var keyContent = PemUtils.StripPemCsrHeader(keyPEM);
            var key = System.Convert.FromBase64String(keyContent);

            var cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportRSAPrivateKey(key, out var _);

            var certificateRequest = new CertificateRequest(subjectName,
                  cryptoServiceProvider, HashAlgorithmName.SHA256,
                  RSASignaturePadding.Pkcs1);

            var csr = PemUtils.DERtoPEM(
                  certificateRequest.CreateSigningRequest(
                     X509SignatureGenerator.CreateForRSA(
                        cryptoServiceProvider,
                        RSASignaturePadding.Pkcs1)), "CERTIFICATE REQUEST");

            using var writer = new StreamWriter(options.CsrFile);
            await writer.WriteAsync(csr);
            await writer.FlushAsync();
            return csr;
        }

    }
}