using FluentValidation;
using Microsoft.Extensions.Localization;
using Server.Core.ArticleAggregate;
using Server.Core.TagAggregate;
using Server.SharedKernel.Resources;

namespace Server.Web.Articles.Create;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator(IStringLocalizer<SharedResource> localizer)
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .MaximumLength(Article.TitleMaxLength)
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .MaximumLength(Article.DescriptionMaxLength)
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .OverridePropertyName("body");

    // Add individual tag validation for better error messages
    RuleForEach(x => x.Article.TagList)
      .Must(tag => !string.IsNullOrWhiteSpace(tag))
      .Must(tag => !tag.Contains(","))
      .WithMessage(x => localizer[SharedResource.Keys.TagCannotContainCommas])
      .MaximumLength(Tag.NameMaxLength);
  }
}
