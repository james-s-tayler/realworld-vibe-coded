using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class ListArticlesRequestValidator : Validator<ListArticlesRequest>
{
  public ListArticlesRequestValidator()
  {
    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .WithMessage("must be greater than 0")
      .LessThanOrEqualTo(100)
      .WithMessage("must be less than or equal to 100");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .WithMessage("must be greater than or equal to 0");

    // Validate string parameters for empty values (but allow null)
    RuleFor(x => x.Tag)
      .Must(tag => tag == null || !string.IsNullOrEmpty(tag))
      .WithMessage("cannot be empty")
      .OverridePropertyName("tag");

    RuleFor(x => x.Author)
      .Must(author => author == null || !string.IsNullOrEmpty(author))
      .WithMessage("cannot be empty")
      .OverridePropertyName("author");

    RuleFor(x => x.Favorited)
      .Must(favorited => favorited == null || !string.IsNullOrEmpty(favorited))
      .WithMessage("cannot be empty")
      .OverridePropertyName("favorited");
  }
}