using FluentValidation;

namespace Server.Web.Articles.Comments.Create;

public class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Comment.Body)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("body");
  }
}
