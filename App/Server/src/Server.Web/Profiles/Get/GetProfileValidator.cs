using FluentValidation;

namespace Server.Web.Profiles.Get;

public class GetProfileValidator : Validator<GetProfileRequest>
{
  public GetProfileValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Username)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("username");
  }
}
