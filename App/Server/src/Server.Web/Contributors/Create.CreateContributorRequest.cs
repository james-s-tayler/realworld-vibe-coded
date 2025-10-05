using System.ComponentModel.DataAnnotations;

namespace Server.Web.Contributors;

public class CreateContributorRequest
{
  public const string Route = "/api/contributors";

  [Required]
  public string? Name { get; set; }
  public string? PhoneNumber { get; set; }
}
