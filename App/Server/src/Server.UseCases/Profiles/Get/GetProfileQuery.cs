using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Get;

public record GetProfileQuery(
  string Username,
  Guid? CurrentUserId = null
) : IQuery<User>;
