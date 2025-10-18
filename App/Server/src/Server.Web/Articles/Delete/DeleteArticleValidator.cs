using FluentValidation;

namespace Server.Web.Articles.Delete;

public class DeleteArticleValidator : Validator<DeleteArticleRequest>
{
  public DeleteArticleValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");
  }
}
