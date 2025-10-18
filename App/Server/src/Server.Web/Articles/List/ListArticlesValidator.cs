using FluentValidation;

namespace Server.Web.Articles.List;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
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

    RuleFor(x => x.Tag)
      .Must(tag => tag == null || tag.Length > 0)
      .WithMessage("cannot be empty")
      .OverridePropertyName("tag");

    RuleFor(x => x.Author)
      .Must(author => author == null || author.Length > 0)
      .WithMessage("cannot be empty")
      .OverridePropertyName("author");

    RuleFor(x => x.Favorited)
      .Must(favorited => favorited == null || favorited.Length > 0)
      .WithMessage("cannot be empty")
      .OverridePropertyName("favorited");
  }
}
