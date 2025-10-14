using FluentValidation;

namespace Server.Web.Articles;

public class FeedValidator : Validator<FeedRequest>
{
  public FeedValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("limit must be greater than 0");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("offset must be greater than or equal to 0");
  }
}
