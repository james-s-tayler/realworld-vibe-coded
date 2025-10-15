using FluentValidation;

namespace Server.Web.Articles.Comments;

public class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Comment.Body)
      .NotEmpty()
      .WithMessage("must not be empty")
      .OverridePropertyName("body");
  }
}
