namespace Server.UseCases.Users;

public record UserDto(int Id, string Email, string Username, string Bio, string? Image, string Token);