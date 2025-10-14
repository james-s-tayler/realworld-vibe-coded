using FluentValidation;

namespace Server.Web.Articles.Comments;

public class DeleteCommentValidator : Validator<DeleteCommentRequest>
{
  public DeleteCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("is required")
      .OverridePropertyName("slug");

    RuleFor(x => x.Id)
      .GreaterThan(0)
      .WithMessage("is invalid")
      .OverridePropertyName("id");
  }
}
