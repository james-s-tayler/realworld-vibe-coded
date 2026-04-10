using FluentValidation;

namespace Server.Web.Articles.Unfavorite;

public class UnfavoriteArticleValidator : Validator<UnfavoriteArticleRequest>
{
  public UnfavoriteArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .OverridePropertyName("slug");
  }
}
