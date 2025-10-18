using FluentValidation;

namespace Server.Web.Articles.Favorite;

public class FavoriteArticleValidator : Validator<FavoriteArticleRequest>
{
  public FavoriteArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");
  }
}
