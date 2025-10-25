using Server.Core.UserAggregate;

namespace Server.UseCases.Users.GetCurrent;

public record GetCurrentUserQuery(Guid UserId) : IQuery<User>;
