namespace Server.Web.Identity.Register;

public class RegisterRequest
{
  public string Email { get; set; } = default!;

  public string Password { get; set; } = default!;

  public string TenantId { get; set; } = default!;
}
