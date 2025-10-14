using FluentValidation;

namespace Server.Web.Articles;

public class FeedValidator : Validator<FeedRequest>
{
  public FeedValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("must be greater than 0")
      .LessThanOrEqualTo(100)
      .WithMessage("must be less than or equal to 100")
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("must be greater than or equal to 0")
      .OverridePropertyName("offset");
  }
}
