using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class UpdateArticleValidator : Validator<UpdateArticleRequest>
{
  public UpdateArticleValidator()
  {
    // Only validate non-empty fields - if a field is provided, it can't be empty
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("title can't be blank")
      .When(x => x.Article.Title != null);

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("description can't be blank")
      .When(x => x.Article.Description != null);

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("body can't be blank")
      .When(x => x.Article.Body != null);

    // Ensure at least one field is provided for update
    RuleFor(x => x.Article)
      .Must(article => article.Title != null || article.Description != null || article.Body != null)
      .WithMessage("article must have at least one field")
      .WithName("article");
  }
}
