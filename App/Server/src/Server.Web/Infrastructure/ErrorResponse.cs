namespace Server.Web.Infrastructure;

/// <summary>
/// Standard error response format for RealWorld API
/// </summary>
public class ConduitErrorResponse
{
  public ConduitErrorBody Errors { get; set; } = new();
}

public class ConduitErrorBody
{
  public string[] Body { get; set; } = Array.Empty<string>();
}
