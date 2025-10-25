using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Update;

public record UpdateUserCommand(
  Guid UserId,
  string? Email = null,
  string? Username = null,
  string? Password = null,
  string? Bio = null,
  string? Image = null
) : ICommand<User>;
