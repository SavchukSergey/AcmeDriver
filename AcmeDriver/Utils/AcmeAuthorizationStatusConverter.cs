using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcmeDriver.Utils {
    public class AcmeAuthorizationStatusConverter : JsonConverter<AcmeAuthorizationStatus> {

        public override AcmeAuthorizationStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String) {
                throw new JsonException();
            }
            var value = reader.GetString();
            return value switch {
                "invalid" => AcmeAuthorizationStatus.Invalid,
                "pending" => AcmeAuthorizationStatus.Pending,
                "valid" => AcmeAuthorizationStatus.Valid,
                "deactivated" => AcmeAuthorizationStatus.Deactivated,
                _ => throw new NotSupportedException($"Unknown authorization status {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, AcmeAuthorizationStatus value, JsonSerializerOptions options) {
            writer.WriteStringValue(value switch {
                AcmeAuthorizationStatus.Invalid => "invalid",
                AcmeAuthorizationStatus.Pending => "pending",
                AcmeAuthorizationStatus.Valid => "valid",
                AcmeAuthorizationStatus.Deactivated => "deactivated",
                _ => throw new NotSupportedException($"Unknown authorization status {value}")
            });
        }
    }
}