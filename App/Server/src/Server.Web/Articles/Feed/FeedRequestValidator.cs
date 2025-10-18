using FluentValidation;

namespace Server.Web.Articles.Feed;

public class FeedRequestValidator : Validator<FeedRequest>
{
  public FeedRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("must be greater than 0")
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("must be greater than or equal to 0")
      .OverridePropertyName("offset");
  }
}
