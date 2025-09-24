using FluentValidation;

namespace Server.UseCases.Articles.Feed;

public class GetFeedValidator : AbstractValidator<GetFeedQuery>
{
  public GetFeedValidator()
  {
    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("limit must be greater than 0");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("offset must be greater than or equal to 0");
  }
}