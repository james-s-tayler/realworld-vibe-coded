namespace Server.Web.DevOnly.Endpoints;

public class RequestValidator : Validator<TestValidationRequest>
{
  public RequestValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("can't be blank");

    RuleFor(x => x.Email).EmailAddress()
      .WithMessage("must be a valid email address");
  }
}
