namespace Server.UseCases.Users.Login;

public record LoginUserQuery(string Email, string Password) : IQuery<Result<UserDto>>;
