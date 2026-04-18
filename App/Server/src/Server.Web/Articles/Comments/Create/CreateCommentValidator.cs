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
      .MaximumLength(Comment.BodyMaxLength);
  }
}
