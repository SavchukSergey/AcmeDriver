using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
    public partial class Program {

        private static async Task<string> GenerateCSRAsync(CommandLineOptions options) {
            if (string.IsNullOrWhiteSpace(options.CsrFile)) {
                throw new CLIException("--csr is required");
            }
            if (string.IsNullOrWhiteSpace(options.Subject)) {
                throw new CLIException("--subject is required");
            }

            var subjectName = options.Subject;

            var privateKey = await LoadPrivateKeyAsync(options);
            var certificateRequest = privateKey switch {
                RSACryptoServiceProvider rsa => new CertificateRequest(subjectName,
                  rsa, HashAlgorithmName.SHA256,
                  RSASignaturePadding.Pkcs1),
                ECDsa ecdsa => new CertificateRequest(subjectName, ecdsa, HashAlgorithmName.SHA256),
                _ => throw new NotSupportedException("Private key is not supported")
            };

            var signatureGenerator = privateKey switch {
                RSACryptoServiceProvider rsa => X509SignatureGenerator.CreateForRSA(
                        rsa,
                        RSASignaturePadding.Pkcs1),
                ECDsa ecdsa => X509SignatureGenerator.CreateForECDsa(ecdsa),
                _ => throw new NotSupportedException("Private key is not supported")
            };

            var csr = PemUtils.DERtoPEM(
                  certificateRequest.CreateSigningRequest(signatureGenerator), "CERTIFICATE REQUEST");

            using var writer = new StreamWriter(options.CsrFile);
            await writer.WriteAsync(csr);
            await writer.FlushAsync();
            return csr;
        }

    }
}