using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AcmeDriver.JWK {
    public class PrivateJwkConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(PrivateJsonWebKey).GetTypeInfo().IsAssignableFrom(objectType);
        }

        public override bool CanWrite => true;

        public override bool CanRead => true;

        protected PrivateJsonWebKey CreateJwk(string type) {
            switch (type) {
                case "RSA":
                    return new RsaPrivateJwk();
                default:
                    throw new NotImplementedException($"Unsupported jwk type {type}");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var obj = JObject.Load(reader);
            string discriminator = (string)obj["kty"];

            var item = CreateJwk(discriminator);
            serializer.Populate(obj.CreateReader(), item);

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value is PrivateJsonWebKey key) {
                var jo = new JObject();
                Type type = value.GetType();

                foreach (PropertyInfo prop in type.GetProperties()) {
                    if (prop.CanRead) {
                        object propVal = prop.GetValue(value, null);
                        if (propVal != null) {
                            jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                        }
                    }
                }
                jo.WriteTo(writer);
            } else {
                writer.WriteNull();
            }
        }

    }
}
