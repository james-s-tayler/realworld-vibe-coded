using FluentValidation;

namespace Server.Web.Articles;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("must be greater than 0");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("must be greater than or equal to 0");

    RuleFor(x => x.Tag)
      .Must(tag => tag == null || tag.Length > 0)
      .WithMessage("cannot be empty");

    RuleFor(x => x.Author)
      .Must(author => author == null || author.Length > 0)
      .WithMessage("cannot be empty");

    RuleFor(x => x.Favorited)
      .Must(favorited => favorited == null || favorited.Length > 0)
      .WithMessage("cannot be empty");
  }
}
