using System;

namespace AcmeDriver {
    public static class PemUtils {

        public static string StripPemCsrHeader(this string content) {
            return content
                .Replace("-----BEGIN NEW CERTIFICATE REQUEST-----", "")
                .Replace("-----END NEW CERTIFICATE REQUEST-----", "")
                .Trim();
        }

        public static byte[] GetPemData(this string content) {
            return Convert.FromBase64String(content);
        }

        public static byte[] GetPemCsrData(this string content) {
            return GetPemData(content.StripPemCsrHeader());
        }
    }
}
