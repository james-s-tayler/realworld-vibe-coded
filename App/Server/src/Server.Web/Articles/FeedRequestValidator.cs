using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class FeedRequestValidator : Validator<FeedRequest>
{
  public FeedRequestValidator()
  {
    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("must be greater than 0")
      .LessThanOrEqualTo(100)
      .WithMessage("must be less than or equal to 100");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("must be greater than or equal to 0");
  }
}