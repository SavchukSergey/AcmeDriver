using System.Text.Json;

namespace AcmeDriver.Utils {
    public static class AcmeJson {

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions {
            Converters = {
                new AcmeOrderStatusConverter(),
                new AcmeAuthorizationStatusConverter()
            }
        };

        public static T Deserialize<T>(string content) {
            //todo: , new PrivateJwkConverter()

            return JsonSerializer.Deserialize<T>(content, _options);
        }

        public static string Serialize<T>(T obj) {
            return JsonSerializer.Serialize(obj, typeof(object), _options);
        }
    }
}