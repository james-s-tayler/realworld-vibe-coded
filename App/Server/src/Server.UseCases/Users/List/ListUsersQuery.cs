using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public record ListUsersQuery() : IQuery<List<ApplicationUser>>;
