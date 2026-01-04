namespace Server.UseCases.Users.Dtos;

public record UserWithRolesDto(
  Guid Id,
  string Email,
  string Username,
  string Bio,
  string? Image,
  List<string> Roles);
