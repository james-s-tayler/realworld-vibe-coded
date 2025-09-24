using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Article.TagList)
      .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && !tag.Contains(",")))
      .WithMessage("must not be empty or contain commas");

    // Add individual tag validation for better error messages
    RuleForEach(x => x.Article.TagList)
      .Must(tag => !string.IsNullOrWhiteSpace(tag))
      .WithMessage("can't be blank")
      .Must(tag => !tag.Contains(","))
      .WithMessage("can't contain commas");
  }
}
