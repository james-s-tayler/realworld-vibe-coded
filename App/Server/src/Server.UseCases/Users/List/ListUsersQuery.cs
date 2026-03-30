using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public record ListUsersQuery(int Limit = 20, int Offset = 0) : IQuery<ListUsersResult>;
