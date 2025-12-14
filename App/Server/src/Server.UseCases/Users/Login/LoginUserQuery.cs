using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Login;

public record LoginUserQuery(string Email, string Password) : IQuery<ApplicationUser>;
