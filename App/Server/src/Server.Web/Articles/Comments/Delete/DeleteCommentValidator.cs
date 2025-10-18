using FluentValidation;

namespace Server.Web.Articles.Comments.Delete;

public class DeleteCommentValidator : Validator<DeleteCommentRequest>
{
  public DeleteCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("can't be blank")
      .OverridePropertyName("slug");
  }
}
