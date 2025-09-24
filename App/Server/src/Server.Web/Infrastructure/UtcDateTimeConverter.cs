using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Web.Infrastructure;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
  public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return DateTime.Parse(reader.GetString()!).ToUniversalTime();
  }

  public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
  {
    // Ensure the DateTime is treated as UTC and formatted with Z suffix
    var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
  }
}
