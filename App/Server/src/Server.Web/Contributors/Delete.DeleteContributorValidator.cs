using FluentValidation;

namespace Server.Web.Contributors;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class DeleteContributorValidator : Validator<DeleteContributorRequest>
{
  public DeleteContributorValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.ContributorId)
      .GreaterThan(0);
  }
}
