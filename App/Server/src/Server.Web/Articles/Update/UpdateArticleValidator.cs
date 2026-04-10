using FluentValidation;
using Microsoft.Extensions.Localization;
using Server.Core.ArticleAggregate;
using Server.SharedKernel.Resources;

namespace Server.Web.Articles.Update;

public class UpdateArticleValidator : Validator<UpdateArticleRequest>
{
  public UpdateArticleValidator(IStringLocalizer<SharedResource> localizer)
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    // Only validate non-empty fields - if a field is provided, it can't be empty
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .MaximumLength(Article.TitleMaxLength)
      .When(x => x.Article.Title != null)
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .MaximumLength(Article.DescriptionMaxLength)
      .When(x => x.Article.Description != null)
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .When(x => x.Article.Body != null)
      .OverridePropertyName("body");

    // Ensure at least one field is provided for update
    RuleFor(x => x.Article)
      .Must(article => article.Title != null || article.Description != null || article.Body != null)
      .WithMessage(x => localizer[SharedResource.Keys.AtLeastOneFieldRequired])
      .OverridePropertyName("article");
  }
}
