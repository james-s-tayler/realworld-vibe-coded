using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleFor(x => x.Limit)
      .GreaterThanOrEqualTo(1)
      .LessThanOrEqualTo(100)
      .WithMessage("Limit must be between 1 and 100");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("Offset must be 0 or greater");
  }
}
