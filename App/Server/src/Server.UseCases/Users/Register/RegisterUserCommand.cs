using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Server.UseCases.Users.Register;

public record RegisterUserCommand(string Email, string Username, string Password) : ICommand<Result<UserDto>>;
