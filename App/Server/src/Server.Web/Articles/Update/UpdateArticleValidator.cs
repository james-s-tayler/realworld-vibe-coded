using FluentValidation;
using Server.Core.ArticleAggregate;

namespace Server.Web.Articles.Update;

public class UpdateArticleValidator : Validator<UpdateArticleRequest>
{
  public UpdateArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Article)
      .Must(a => a.Title != null || a.Description != null || a.Body != null)
      .WithMessage("at least one field must be provided.")
      .OverridePropertyName("article");

    RuleFor(x => x.Article.Title)
      .NotEmpty().WithMessage("is required.")
      .MaximumLength(Article.TitleMaxLength).WithMessage($"cannot exceed {Article.TitleMaxLength} characters.")
      .OverridePropertyName("title")
      .When(x => x.Article.Title != null);

    RuleFor(x => x.Article.Description)
      .NotEmpty().WithMessage("is required.")
      .MaximumLength(Article.DescriptionMaxLength).WithMessage($"cannot exceed {Article.DescriptionMaxLength} characters.")
      .OverridePropertyName("description")
      .When(x => x.Article.Description != null);

    RuleFor(x => x.Article.Body)
      .NotEmpty().WithMessage("is required.")
      .MaximumLength(Article.BodyMaxLength).WithMessage($"cannot exceed {Article.BodyMaxLength} characters.")
      .OverridePropertyName("body")
      .When(x => x.Article.Body != null);
  }
}
