using Server.Core.UserAggregate;

namespace Server.UseCases.Users.Login;

public record LoginUserQuery(string Email, string Password) : IQuery<Result<User>>;
