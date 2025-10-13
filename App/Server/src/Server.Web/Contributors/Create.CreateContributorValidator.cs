using FluentValidation;
using Server.Infrastructure.Data.Config;

namespace Server.Web.Contributors;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class CreateContributorValidator : Validator<CreateContributorRequest>
{
  public CreateContributorValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);
  }
}
