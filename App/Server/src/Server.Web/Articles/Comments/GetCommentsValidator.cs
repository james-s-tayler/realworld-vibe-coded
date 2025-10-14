using FluentValidation;

namespace Server.Web.Articles.Comments;

public class GetCommentsValidator : Validator<GetCommentsRequest>
{
  public GetCommentsValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("is required")
      .OverridePropertyName("slug");
  }
}
