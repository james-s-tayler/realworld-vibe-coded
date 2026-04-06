using System.Text.Json;
using Server.Web.Infrastructure;

namespace Server.UnitTests.Web;

public class UtcDateTimeConverterTests
{
  private readonly JsonSerializerOptions _options;

  public UtcDateTimeConverterTests()
  {
    _options = new JsonSerializerOptions();
    _options.Converters.Add(new UtcDateTimeConverter());
  }

  [Fact]
  public void Write_UtcDateTime_FormatsWithZSuffix()
  {
    var dt = DateTime.Parse("2024-06-15T12:30:45Z").ToUniversalTime();

    var json = JsonSerializer.Serialize(dt, _options);

    json.ShouldContain("2024-06-15T12:30:45.0000000Z");
  }

  [Fact]
  public void Write_UnspecifiedKindDateTime_ConvertsToUtcWithZSuffix()
  {
    var dt = DateTime.Parse("2024-06-15T12:30:45");

    var json = JsonSerializer.Serialize(dt, _options);

    json.ShouldEndWith("Z\"");
  }

  [Fact]
  public void Read_IsoDateTimeString_ParsesAsUtc()
  {
    var json = "\"2024-06-15T12:30:45Z\"";

    var dt = JsonSerializer.Deserialize<DateTime>(json, _options);

    dt.Kind.ShouldBe(DateTimeKind.Utc);
    dt.Year.ShouldBe(2024);
    dt.Month.ShouldBe(6);
    dt.Day.ShouldBe(15);
    dt.Hour.ShouldBe(12);
    dt.Minute.ShouldBe(30);
  }

  [Fact]
  public void Read_OffsetDateTimeString_ConvertsToUtc()
  {
    var json = "\"2024-06-15T14:30:45+02:00\"";

    var dt = JsonSerializer.Deserialize<DateTime>(json, _options);

    dt.Kind.ShouldBe(DateTimeKind.Utc);
    dt.Hour.ShouldBe(12);
  }

  [Fact]
  public void RoundTrip_PreservesUtcDateTime()
  {
    var original = DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime();

    var json = JsonSerializer.Serialize(original, _options);
    var deserialized = JsonSerializer.Deserialize<DateTime>(json, _options);

    deserialized.Kind.ShouldBe(DateTimeKind.Utc);
    deserialized.ShouldBe(original);
  }
}
