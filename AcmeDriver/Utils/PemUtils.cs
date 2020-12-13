using System;
using System.Text;

namespace AcmeDriver {
    public static class PemUtils {

        public static string StripPemCsrHeader(this string content) {
            return content
                .Replace("-----BEGIN NEW CERTIFICATE REQUEST-----", "")
                .Replace("-----END NEW CERTIFICATE REQUEST-----", "")
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
                .Replace("-----END CERTIFICATE REQUEST-----", "")
                .Trim();
        }

        public static byte[] GetPemData(this string content) {
            return Convert.FromBase64String(content);
        }

        public static byte[] GetPemCsrData(this string content) {
            return GetPemData(content.StripPemCsrHeader());
        }

        public static string DERtoPEM(byte[] bytesDER, string type) {
            var builder = new StringBuilder();
            builder.AppendLine($"-----BEGIN {type}-----");

            var base64 = Convert.ToBase64String(bytesDER);

            int offset = 0;
            const int LineLength = 64;
            while (offset < base64.Length) {
                int lineEnd = Math.Min(offset + LineLength, base64.Length);
                builder.AppendLine(
                   base64.Substring(offset, lineEnd - offset));
                offset = lineEnd;
            }

            builder.AppendLine($"-----END {type}-----");
            return builder.ToString();
        }

    }
}
