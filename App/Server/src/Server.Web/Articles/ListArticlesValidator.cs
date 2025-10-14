using FluentValidation;

namespace Server.Web.Articles;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
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

    RuleFor(x => x.Tag)
      .NotEmpty()
      .When(x => x.Tag != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("tag");

    RuleFor(x => x.Author)
      .NotEmpty()
      .When(x => x.Author != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("author");

    RuleFor(x => x.Favorited)
      .NotEmpty()
      .When(x => x.Favorited != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("favorited");
  }
}
