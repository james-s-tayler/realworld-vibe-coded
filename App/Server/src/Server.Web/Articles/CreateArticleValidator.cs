using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("Title is required")
      .MaximumLength(200)
      .WithMessage("Title must not exceed 200 characters");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("Description is required")
      .MaximumLength(500)
      .WithMessage("Description must not exceed 500 characters");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("Body is required");

    RuleFor(x => x.Article.TagList)
      .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && !tag.Contains(",")))
      .WithMessage("Tags must not be empty or contain commas");
  }
}