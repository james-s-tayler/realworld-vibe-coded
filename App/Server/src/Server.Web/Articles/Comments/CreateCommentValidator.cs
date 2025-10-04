using FluentValidation;

namespace Server.Web.Articles.Comments;

public class CreateCommentValidator : Validator<CreateCommentRequest>
{
  public CreateCommentValidator()
  {
    RuleFor(x => x.Comment.Body)
      .NotEmpty()
      .WithMessage("can't be blank");
  }
}
