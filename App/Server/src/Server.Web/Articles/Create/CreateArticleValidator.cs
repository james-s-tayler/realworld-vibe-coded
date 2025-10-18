using FluentValidation;

namespace Server.Web.Articles.Create;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank")
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
      .WithMessage("can't contain commas");
  }
}
