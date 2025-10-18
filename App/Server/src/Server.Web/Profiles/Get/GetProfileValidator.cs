using FluentValidation;

namespace Server.Web.Profiles.Get;

public class GetProfileValidator : Validator<GetProfileRequest>
{
  public GetProfileValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty()
      .WithMessage("is required.")
      .OverridePropertyName("username");
  }
}
