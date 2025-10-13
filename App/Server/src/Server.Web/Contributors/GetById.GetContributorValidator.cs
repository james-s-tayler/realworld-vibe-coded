using FluentValidation;

namespace Server.Web.Contributors;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class GetContributorValidator : Validator<GetContributorByIdRequest>
{
  public GetContributorValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.ContributorId)
      .GreaterThan(0);
  }
}
