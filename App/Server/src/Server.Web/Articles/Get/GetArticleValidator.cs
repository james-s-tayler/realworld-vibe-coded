using FluentValidation;

namespace Server.Web.Articles.Get;

public class GetArticleValidator : Validator<GetArticleRequest>
{
  public GetArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");
  }
}
