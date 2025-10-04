namespace Server.UseCases.Users.GetCurrent;

public record GetCurrentUserQuery(int UserId) : IQuery<Result<UserDto>>;
