using FluentValidation;

namespace Server.Web.Identity.Invite;

public class InviteRequestValidator : Validator<InviteRequest>
{
  public InviteRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(6);
  }
}
