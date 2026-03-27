using FluentValidation;

namespace Server.Web.Articles.List;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Tag)
      .NotEmpty().WithMessage("must not be empty.")
      .OverridePropertyName("tag")
      .When(x => x.Tag != null);

    RuleFor(x => x.Author)
      .NotEmpty().WithMessage("must not be empty.")
      .OverridePropertyName("author")
      .When(x => x.Author != null);

    RuleFor(x => x.Favorited)
      .NotEmpty().WithMessage("must not be empty.")
      .OverridePropertyName("favorited")
      .When(x => x.Favorited != null);

    RuleFor(x => x.Limit)
      .GreaterThanOrEqualTo(1).WithMessage("must be at least 1.")
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0).WithMessage("must be at least 0.")
      .OverridePropertyName("offset");
  }
}
