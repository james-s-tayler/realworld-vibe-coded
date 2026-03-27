using FluentValidation;

namespace Server.Web.Articles.Create;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Article.Title)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("body");

    RuleForEach(x => x.Article.TagList)
      .NotEmpty().WithMessage("must not be empty.")
      .Must(t => !t.Contains(',')).WithMessage("must not contain commas.");
  }
}
