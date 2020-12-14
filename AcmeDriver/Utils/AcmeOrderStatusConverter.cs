using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcmeDriver.Utils {
    public class AcmeOrderStatusConverter : JsonConverter<AcmeOrderStatus> {

        public override AcmeOrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String) {
                throw new JsonException();
            }
            var value = reader.GetString();
            return value switch {
                "invalid" => AcmeOrderStatus.Invalid,
                "pending" => AcmeOrderStatus.Pending,
                "processing" => AcmeOrderStatus.Processing,
                "valid" => AcmeOrderStatus.Valid,
                "ready" => AcmeOrderStatus.Ready,
                _ => throw new NotSupportedException($"Unknown order status {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, AcmeOrderStatus value, JsonSerializerOptions options) {
            writer.WriteStringValue(value switch {
                AcmeOrderStatus.Invalid => "invalid",
                AcmeOrderStatus.Pending => "pending",
                AcmeOrderStatus.Processing => "processing",
                AcmeOrderStatus.Valid => "valid",
                AcmeOrderStatus.Ready => "ready",
                _ => throw new NotSupportedException($"Unknown order status {value}")
            });
        }
    }
}