using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class CreateArticleValidator : Validator<CreateArticleRequest>
{
  public CreateArticleValidator()
  {
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("title is required and cannot be empty")
      .OverridePropertyName("title");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("description is required and cannot be empty")
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
      .WithMessage("taglist[{CollectionIndex}] is required and cannot be empty")
      .Must(tag => !tag.Contains(","))
      .WithMessage("taglist[{CollectionIndex}] cannot contain commas");
  }
}
