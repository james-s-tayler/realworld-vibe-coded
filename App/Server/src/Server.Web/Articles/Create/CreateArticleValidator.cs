using FluentValidation;
using Server.Core.ArticleAggregate;
using Server.Core.TagAggregate;

namespace Server.Web.Articles.Create;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(Article.TitleMaxLength)
      .WithMessage($"cannot exceed {Article.TitleMaxLength} characters")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(Article.DescriptionMaxLength)
      .WithMessage($"cannot exceed {Article.DescriptionMaxLength} characters")
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("body");

    // Add individual tag validation for better error messages
    RuleForEach(x => x.Article.TagList)
      .Must(tag => !string.IsNullOrWhiteSpace(tag))
      .WithMessage("can't be blank")
      .Must(tag => !tag.Contains(","))
      .WithMessage("can't contain commas")
      .MaximumLength(Tag.NameMaxLength)
      .WithMessage($"cannot exceed {Tag.NameMaxLength} characters");
  }
}
