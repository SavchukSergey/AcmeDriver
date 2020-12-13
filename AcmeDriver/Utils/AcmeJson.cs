using System.Text.Json;

namespace AcmeDriver.Utils {
    public static class AcmeJson {

        public static T Deserialize<T>(string content) {
            //todo: , new PrivateJwkConverter()
            return JsonSerializer.Deserialize<T>(content);
        }

        public static string Serialize<T>(T obj) {
            return JsonSerializer.Serialize(obj);
        }
    }
}