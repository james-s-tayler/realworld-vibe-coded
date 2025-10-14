using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Register;

public record RegisterUserCommand(string Email, string Username, string Password) : ICommand<Result<User>>;
