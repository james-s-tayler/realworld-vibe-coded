using FluentValidation;

namespace Server.Web.Articles.Get;

public class GetArticleValidator : Validator<GetArticleRequest>
{
  public GetArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .OverridePropertyName("slug");
  }
}
