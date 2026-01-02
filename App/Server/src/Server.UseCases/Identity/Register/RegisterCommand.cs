using Destructurama.Attributed;
using MediatR;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Register;

public class RegisterCommand : ICommand<Unit>
{
  public required string Email { get; set; }

  [NotLogged]
  public required string Password { get; set; }
}
