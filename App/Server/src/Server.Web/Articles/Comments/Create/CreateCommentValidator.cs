using FluentValidation;
using Server.Core.ArticleAggregate;

namespace Server.Web.Articles.Comments.Create;

public class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Comment.Body)
      .NotEmpty()
      .WithMessage("can't be blank")
      .MaximumLength(Comment.BodyMaxLength)
      .WithMessage($"cannot exceed {Comment.BodyMaxLength} characters")
      .OverridePropertyName("body");
  }
}
