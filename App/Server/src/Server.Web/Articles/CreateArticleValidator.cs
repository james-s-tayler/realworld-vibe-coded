using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleFor(x => x.Article)
      .NotNull()
      .WithMessage("Article data is required");

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
      .WithMessage("Body is required")
      .MaximumLength(10000)
      .WithMessage("Body must not exceed 10000 characters");

    RuleForEach(x => x.Article.TagList)
      .NotEmpty()
      .WithMessage("Tag cannot be empty")
      .MaximumLength(50)
      .WithMessage("Tag must not exceed 50 characters")
      .Must(tag => !tag.Contains(","))
      .WithMessage("Tag cannot contain commas");
  }
}