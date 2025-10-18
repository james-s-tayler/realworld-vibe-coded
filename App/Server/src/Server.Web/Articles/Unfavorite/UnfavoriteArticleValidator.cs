using FluentValidation;

namespace Server.Web.Articles.Unfavorite;

public class UnfavoriteArticleValidator : Validator<UnfavoriteArticleRequest>
{
  public UnfavoriteArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");
  }
}
