using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Login;

public record LoginUserQuery(string Email, string Password) : IQuery<User>;
