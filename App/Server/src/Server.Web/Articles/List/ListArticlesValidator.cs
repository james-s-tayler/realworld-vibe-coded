using FluentValidation;

namespace Server.Web.Articles.List;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .OverridePropertyName("offset");

    RuleFor(x => x.Tag)
      .NotEmpty()
      .When(x => x.Tag != null)
      .OverridePropertyName("tag");

    RuleFor(x => x.Author)
      .NotEmpty()
      .When(x => x.Author != null)
      .OverridePropertyName("author");

    RuleFor(x => x.Favorited)
      .NotEmpty()
      .When(x => x.Favorited != null)
      .OverridePropertyName("favorited");
  }
}
