using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Server.UseCases.Users.FollowProfile;

public record FollowProfileCommand(int UserId, string UsernameToFollow) : ICommand<Result<ProfileDto>>;