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
      .WithMessage("can't be blank")
      .MaximumLength(200)
      .WithMessage("is too long (maximum is 200 characters)")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(500)
      .WithMessage("is too long (maximum is 500 characters)")
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(10000)
      .WithMessage("is too long (maximum is 10000 characters)")
      .OverridePropertyName("body");

    RuleForEach(x => x.Article.TagList)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(50)
      .WithMessage("is too long (maximum is 50 characters)")
      .Must(tag => !tag.Contains(","))
      .WithMessage("cannot contain commas");
  }
}
