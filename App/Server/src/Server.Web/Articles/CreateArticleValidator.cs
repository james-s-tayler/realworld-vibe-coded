using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
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

    RuleFor(x => x.Article.TagList)
      .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && !tag.Contains(",")))
      .WithMessage("must not be empty or contain commas")
      .OverridePropertyName("tagList");
  }
}
