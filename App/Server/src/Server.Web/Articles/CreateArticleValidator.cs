using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("title can't be blank")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("description can't be blank")
      .OverridePropertyName("description");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("body can't be blank")
      .OverridePropertyName("body");

    RuleFor(x => x.Article.TagList)
      .Must(tags => tags == null || tags.All(tag => !string.IsNullOrWhiteSpace(tag) && !tag.Contains(",")))
      .WithMessage("must not be empty or contain commas")
      .OverridePropertyName("tagList");

    // Add individual tag validation for better error messages
    RuleForEach(x => x.Article.TagList)
      .Must(tag => !string.IsNullOrWhiteSpace(tag))
      .WithMessage("tagList[{CollectionIndex}] can't be blank")
      .Must(tag => !tag.Contains(","))
      .WithMessage("tagList[{CollectionIndex}] can't contain commas");
  }
}
