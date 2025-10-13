using FluentValidation;

namespace Server.Web.Articles;

public class UpdateArticleValidator : Validator<UpdateArticleRequest>
{
  public UpdateArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    // Only validate non-empty fields - if a field is provided, it can't be empty
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("can't be blank")
      .When(x => x.Article.Title != null)
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank")
      .When(x => x.Article.Description != null)
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("can't be blank")
      .When(x => x.Article.Body != null)
      .OverridePropertyName("body");

    // Ensure at least one field is provided for update
    RuleFor(x => x.Article)
      .Must(article => article.Title != null || article.Description != null || article.Body != null)
      .WithMessage("must have at least one field")
      .OverridePropertyName("article");
  }
}
