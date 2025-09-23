using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Server.UseCases.Users.Update;

public record UpdateUserCommand(
  int UserId,
  string? Email = null,
  string? Username = null,
  string? Password = null,
  string? Bio = null,
  string? Image = null
) : ICommand<Result<UserDto>>;
