using FluentValidation;

namespace Server.Web.Articles.Feed;

public class FeedArticlesValidator : Validator<FeedArticlesRequest>
{
  public FeedArticlesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThanOrEqualTo(1).WithMessage("must be at least 1.")
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0).WithMessage("must be at least 0.")
      .OverridePropertyName("offset");
  }
}
