using FluentValidation;

namespace Server.Web.Articles.Comments.Get;

public class GetCommentsValidator : Validator<GetCommentsRequest>
{
  public GetCommentsValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty();
  }
}
