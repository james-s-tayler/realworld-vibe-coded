using FluentValidation;

namespace Server.Web.Articles.Comments;

public class DeleteCommentValidator : Validator<DeleteCommentRequest>
{
  public DeleteCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");

    RuleFor(x => x.Id)
      .GreaterThan(0)
      .WithMessage("must be valid")
      .OverridePropertyName("id");
  }
}
