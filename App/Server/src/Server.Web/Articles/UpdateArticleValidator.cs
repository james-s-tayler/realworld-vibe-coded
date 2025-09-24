using FastEndpoints;
using FluentValidation;

namespace Server.Web.Articles;

public class UpdateArticleValidator : Validator<UpdateArticleRequest>
{
  public UpdateArticleValidator()
  {
    RuleFor(x => x.Article.Title)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Article.Description)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Article.Body)
      .NotEmpty()
      .WithMessage("can't be blank");
  }
}
