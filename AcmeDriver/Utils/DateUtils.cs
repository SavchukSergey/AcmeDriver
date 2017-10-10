using System;

namespace AcmeDriver {
    public static class DateUtils {

        public static string ToRfc3339String(this DateTime date) {
            return date.ToString("yyyy-MM-dd'T'HH:mm:ssZ");
        }

        public static string ToRfc3339String(this DateTimeOffset date) {
            return date.UtcDateTime.ToRfc3339String();
        }

    }
}
