using FluentValidation;

namespace Server.Web.Comments.Create;

public class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Comment.Body)
      .NotEmpty().WithMessage("is required.")
      .OverridePropertyName("body");
  }
}
