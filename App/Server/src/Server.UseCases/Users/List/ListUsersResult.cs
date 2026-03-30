using Server.UseCases.Users.Dtos;

namespace Server.UseCases.Users.List;

public record ListUsersResult(List<UserWithRolesDto> Users, int TotalCount);
